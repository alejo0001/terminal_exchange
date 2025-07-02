using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Caching;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.Settings;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;

/// <summary>
///     <inheritdoc cref="ITopSellerCourseService" /><br />
///     Implementation: <em>X=10</em>.
/// </summary>
/// <remarks>
///     Implements Cache-Aside pattern to get/set cache and query database if key not found in cache.<br />
///     Race condition on cache loading from database is managed using <see cref="SemaphoreSlim" /> based
///     locking mechanism.<br />
/// </remarks>
public class TopSellerCourseService
    : AbstractMemoryCache<
            TopSellerCourseCacheKey,
            IDictionary<TopSellerCoursesStatsDto, Course>,
            TopSellerCourseSettings>,
        ITopSellerCourseService
{
    private const string CacheKeyPrefix = nameof(TopSellerCourseService);

    private static readonly SemaphoreSlim Semaphore;

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ITlmkDbContext _tlmkDbContext;
    private readonly TopSellerCourseSettings _settings;
    private readonly ILogger<TopSellerCourseService> _logger;

    static TopSellerCourseService() => Semaphore = new(1, 1);

    public TopSellerCourseService(
        IMemoryCache memoryCache,
        IApplicationDbContext applicationDbContext,
        ITlmkDbContext tlmkDbContext,
        IOptionsSnapshot<TopSellerCourseSettings> options,
        ILogger<TopSellerCourseService> logger)
        : base(memoryCache, options)
    {
        _applicationDbContext = applicationDbContext;
        _tlmkDbContext = tlmkDbContext;
        _logger = logger;

        _settings = options.Value;

        (DateStart, DateEnd) = CalculatePeriod(options.Value);
    }

    public DateTime DateStart { get; }

    public DateTime DateEnd { get; }

    /// <inheritdoc />
    public override object GetKey(TopSellerCourseCacheKey key) =>
        $"{CacheKeyPrefix}/{key.FacultyName}-{key.CountryCode}";

    /// <summary>
    ///     Gets TOP10 sold courses statistics from TLMK db and gets corresponding Courses data from Intranet DB
    ///     and puts them together into cache.<br />
    ///     Cache is used in future to retrieve data from there instead of databases of TLMK and Intranet.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ct"></param>
    /// <remarks>Cache-Aside pattern.</remarks>
    /// <returns>A dictionary of statistics DTO and it's corresponding course.</returns>
    /// <inheritdoc />
    public override async ValueTask<IDictionary<TopSellerCoursesStatsDto, Course>?> Get(
        TopSellerCourseCacheKey key,
        CancellationToken ct)
    {
        if (await base.Get(key, ct).ConfigureAwait(false) is { } item)
        {
            _logger.LogDebug("Cache had item to return");

            return item;
        }

        return await LoadToCache(key, ct).ConfigureAwait(false);
    }

    private async Task<IDictionary<TopSellerCoursesStatsDto, Course>> LoadToCache(
        TopSellerCourseCacheKey key,
        CancellationToken ct)
    {
        using var _0 = _logger.BeginScope("LoadToCache for {@Key}", key);
        _logger.LogDebug("Begin `LoadCache` operation");

        try
        {
            await Semaphore.WaitAsync(ct);

            if (await base.Get(key, ct).ConfigureAwait(false) is { } value)
            {
                _logger.LogDebug("Entering semaphore was delayed, returning item, that was loaded by other thread. ");

                return value;
            }

            _logger.LogDebug("Being loading of `statistics data` from TLMK database");

            var statisticsQuery = GetTop10SellersStatisticsQuery(key.CountryCode, key.FacultyName);
            var facultyTopSellers = await statisticsQuery.ToListAsync(ct);

            _logger.LogDebug("Finished loading of `statistics data` from TLMK database");

            var coursesData = new Dictionary<TopSellerCoursesStatsDto, Course>();

            if (facultyTopSellers.Count == 0)
            {
                _logger.LogDebug(
                    "No usable statistics data for given criteria: store fact to cache to avoid repeating load");

                return await base.Set(key, coursesData, ct).ConfigureAwait(false);
            }

            _logger.LogDebug("Being loading of corresponding Courses data from Intranet database");

            var courseInfoQuery = GetCourseInfoQuery(facultyTopSellers);
            var courses = await MaterializeAsLongRunning(courseInfoQuery, ct);

            _logger.LogDebug("Finished loading of corresponding Courses data from Intranet database");

            // TODO: 2. should  update existing cache data if possible, rather than adding plain copy, BUT
            // analyze whether it is reasonable to do, because of the way cache entries are retrieved (key!)
            courses.ForEach(
                c =>
                {
                    if (HasIncompleteGraph(c))
                    {
                        return;
                    }

                    var statsDto = facultyTopSellers.FirstOrDefault(
                        s => s.CourseCode.Equals(c.Code, StringComparison.InvariantCultureIgnoreCase));

                    if (statsDto is { })
                    {
                        coursesData[statsDto] = c;
                    }
                });

            return await base.Set(key, coursesData, ct).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();

            _logger.LogDebug("Finished `LoadCache` operation");
        }
    }

    private static bool HasIncompleteGraph(Course c) =>
        c.CourseType is null
        || !c.CourseFaculties.Any()
        || !c.CourseDataList.Any();

    private IQueryable<TopSellerCoursesStatsDto> GetTop10SellersStatisticsQuery(string country, string facultyName)
    {
        var query = _tlmkDbContext.PedidosTlmk.AsNoTracking()
            .Where(p => p.Precio > 0 || p.Precio_Final > 0)
            .Where(p => p.Area!.ToUpper() == facultyName.ToUpper())
            .Where(p => p.Pais!.ToUpper() == country.ToUpper())
            .Where(p => !_settings.ExcludedPaymentTypes.Contains(p.TipoPago))
            .Where(p => !EF.Functions.Like(p.TipoPago!, _settings.ExcludePaymentTypePattern))
            .Where(p => DateStart <= p.FechaPedido && p.FechaPedido <= DateEnd)
            .GroupBy(p => new { p.Area, p.CourseCode, p.Pais, p.Precio, p.Precio_Final })
            .Select(
                grp => new TopSellerCoursesStatsDto
                {
                    FacultyName = grp.Key.Area!,
                    CourseCode = grp.Key.CourseCode!,
                    CourseCountryCode = grp.Key.Pais!,
                    SoldCount = grp.Count(),
                    MaxPrice = (decimal)grp.Max(p => p.Precio)!,
                    MaxPriceFinal = (decimal)grp.Max(p => p.Precio_Final)!,
                })
            .OrderBy(grp => grp.SoldCount)
            .ThenByDescending(rp => rp.MaxPrice)
            .ThenByDescending(rp => rp.MaxPriceFinal)
            .Take(_settings.StatisticsTakeLimit);

        return query;
    }

    /// <summary>
    ///     Encapsulates logic to convert settings into concrete start and end of the period, that is used in statistics
    ///     calculation query.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static (DateTime Start, DateTime End) CalculatePeriod(TopSellerCourseSettings settings)
    {
        var utcNow = DateTime.UtcNow;

        DateTime start;
        if (settings.StatisticsPeriodStart is { })
        {
            start = settings.StatisticsPeriodStart.Value;
        }
        else if (settings.StatisticsPeriod is { })
        {
            start = utcNow.AddDays(settings.StatisticsPeriod.Value.Negate().Days);
        }
        else
        {
            start = utcNow.AddMonths(-TopSellerCourseSettings.DefaultMonthsBack);
        }

        var end = settings.StatisticsPeriodEnd ?? utcNow;

        return (start, end);
    }

    /// <summary>
    ///     Get query for getting necessary information regarding Course, e.g. "includes".
    /// </summary>
    /// <param name="statisticsList"></param>
    private IQueryable<Course> GetCourseInfoQuery(IList<TopSellerCoursesStatsDto> statisticsList)
    {
        // TODO: Generated SQL reveals need to Distinct this data!
        var courseCodes = statisticsList.Select(s => s.CourseCode);
        var courseCountryCodes = statisticsList.Select(s => s.CourseCountryCode);
        var facultyNames = statisticsList.Select(s => s.FacultyName);

        // TODO: Optimize: project data so to minimize cacheable data. Only a fraction of all retrieved graph is needed!

        var query = _applicationDbContext.Courses.AsNoTracking().AsSplitQuery()
            .Include(c => c.CourseType)
            .Include(c => c.CourseFaculties
                .Where(cf =>
                    !cf.IsDeleted
                    && facultyNames.Contains(cf.Faculty.Name.ToUpper())))
            .ThenInclude(f => f.Faculty)
            .Include(c => c.CourseDataList // TODO: Project?
                .Where(cd =>
                    !cd.IsDeleted
                    && courseCountryCodes.Contains(cd.CourseCountry.Code.ToUpper())))
            .ThenInclude(cd => cd.CourseCountry)
            .Where(c => !c.IsDeleted)
            .Where(c => courseCodes.Contains(c.Code))
            .Where(c => !string.IsNullOrWhiteSpace(c.CourseType.Name))
            .Where(c => c.CourseFaculties.Any(cf =>
                !cf.IsDeleted
                && facultyNames.Contains(cf.Faculty.Name.ToUpper())))
            .Where(c => c.CourseDataList.Any(cd =>
                !cd.IsDeleted
                && courseCountryCodes.Contains(cd.CourseCountry.Code.ToUpper())));

        return query;
    }

    private async Task<List<TResult>> MaterializeAsLongRunning<TResult>(IQueryable<TResult> query, CancellationToken ct)
    {
        // TODO: there is a reusable & reliable service for this now i Croupier, copy it!
        var originalTimeout = _applicationDbContext.Database.GetCommandTimeout();
        _applicationDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(2)); // Set a 2-minute timeout

        var result = await query.ToListAsync(ct).ConfigureAwait(false);

        _applicationDbContext.Database.SetCommandTimeout(originalTimeout); // Reset to the original timeout

        return result;
    }
}
