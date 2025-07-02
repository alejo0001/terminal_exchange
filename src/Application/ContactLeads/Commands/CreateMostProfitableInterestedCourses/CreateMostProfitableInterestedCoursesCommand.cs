using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;
using CrmAPI.Application.Settings;
using CrmAPI.Contracts.Commands;
using CrmAPI.Contracts.Dtos;
using CrmAPI.Contracts.Events;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrmAPI.Application.ContactLeads.Commands.CreateMostProfitableInterestedCourses;

/// <inheritdoc cref="ICreateMostProfitableInterestedCourses" />
/// <summary>
///     Creates instances of <see cref="ContactLead" /> and <see cref="ContactLeadProcess" />, then broadcasts
///     <see cref="IMostProfitableInterestedCoursesCreated" /> event.
/// </summary>
/// <remarks>
///     NB! No security, only used in messaging communication; add ApiKey ASAP in case of exposing it via WebApi.
/// </remarks>
public sealed record CreateMostProfitableInterestedCoursesCommand(CreateMostProfitableInterestedCoursesDto Dto)
    : IRequest<ErrorOr<MostProfitableInterestedCoursesCreatedDto>>;

[UsedImplicitly]
public sealed class CreateMostProfitableInterestedCoursesCommandHandler
    : IRequestHandler<CreateMostProfitableInterestedCoursesCommand, ErrorOr<MostProfitableInterestedCoursesCreatedDto>>
{
    private const string CreatorTag = "create-most-profitable-interested-courses";
    private const string LogErrNoInterestedCoursesCreated = "Wasn't able to create any interested courses; halt job";

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ITopSellerCourseService _topSellersService;
    private readonly IBus _bus;
    private readonly InterestedCoursePopulatorSettings _settings;
    private readonly ILogger<CreateMostProfitableInterestedCoursesCommandHandler> _logger;

    private readonly string _contactLeadRecommendedType;

    public CreateMostProfitableInterestedCoursesCommandHandler(
        IApplicationDbContext applicationDbContext,
        ITlmkDbContext tlmkDbContext,
        ITopSellerCourseService topSellersService,
        IBus bus,
        IOptionsSnapshot<InterestedCoursePopulatorSettings> options,
        IConfiguration configuration,
        ILogger<CreateMostProfitableInterestedCoursesCommandHandler> logger)
    {
        _applicationDbContext = applicationDbContext;
        _topSellersService = topSellersService;
        _bus = bus;
        _settings = options.Value;
        _logger = logger;

        _contactLeadRecommendedType = configuration["Constants:ContactLeadRecommendedType"]!;
    }

    public async Task<ErrorOr<MostProfitableInterestedCoursesCreatedDto>> Handle(
        CreateMostProfitableInterestedCoursesCommand request,
        CancellationToken ct)
    {
        using var _1 = _logger.BeginScope("{@Dto}", request.Dto);
        _logger.LogTrace("Start");
        request.Dto.Deconstruct(out var contactId, out var processId, out var topCoursesCount);
        // TODO: Add Distributed Caching. LSaar has a branch for implementation ready!

        var contact = await GetContact(contactId, ct);

        if (await GetInterestedCourses(contact, topCoursesCount, ct) is not [_, ..] newInterestedCourses)
        {
            _logger.LogError(LogErrNoInterestedCoursesCreated);

            return Error.Unexpected(description: LogErrNoInterestedCoursesCreated);
        }

        if (newInterestedCourses.Count < topCoursesCount)
        {
            _logger.LogWarning(
                "Not enough source data to create {ExpectedInterestedCoursesCount} courses; "
                + "creating {FoundRawDataCount}; ",
                topCoursesCount,
                newInterestedCourses.Count);
        }

        AddInterestedCourses(newInterestedCourses, processId);

        await _applicationDbContext.SaveChangesAsync(CreatorTag, ct);

        var result = new MostProfitableInterestedCoursesCreatedDto(
            contactId,
            processId,
            newInterestedCourses.Select(cl => cl.Id).ToArray());

        var @event = new { Dto = result, CorrelationId = NewId.Next() };

        _logger.LogTrace("End {@Event}", @event);

        await _bus.Publish<IMostProfitableInterestedCoursesCreated>(@event, ct);

        return result;
    }

    /// <summary>
    ///     Get Contact and required related data.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns>
    ///     <para>
    ///         Has included Faculties, that aren't excluded by
    ///         <see cref="InterestedCoursePopulatorSettings.ExcludedFaculties" />
    ///     </para>
    ///     <para>
    ///         Includes existing <b>Interested Courses</b> from <see cref="Contact.ContactLeads" /> by business rule:<br />
    ///         Definition on "Interested Course": if Contact has ContactLead with either `CourseId` or `CourseCode`.
    ///     </para>
    ///     <para>
    ///         Uses Long Running Query Execution pattern.
    ///     </para>
    /// </returns>
    private async Task<Contact> GetContact(int id, CancellationToken ct)
    {
        var query = _applicationDbContext.Contact
            .AsSplitQuery()
            .Include(
                c => c.Faculties
                    .Where(f => !f.IsDeleted)
                    .Where(f => !_settings.ExcludedFaculties.Contains(f.Name.ToUpper())))
            .Include(
                c => c.ContactLeads
                    .Where(cl => !cl.IsDeleted)
                    .Where(cl => cl.CourseId > 0 || !string.IsNullOrWhiteSpace(cl.CourseCode)))
            .Where(c => c.Id == id)
            .Where(c => !c.IsDeleted);

        var originalTimeout = _applicationDbContext.Database.GetCommandTimeout();
        _applicationDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(2)); // Set a 2-minute timeout

        var result = await query.FirstAsync(ct);

        _applicationDbContext.Database.SetCommandTimeout(originalTimeout); // Reset to the original timeout

        return result;
    }

    private async Task<List<ContactLead>> GetInterestedCourses(Contact contact, int limit, CancellationToken ct)
    {
        var (result, potentialData) = await CreateInterestedCoursesOverAllFaculties(contact, limit, ct);

        return result.Count >= limit
            ? result
            : CreateInterestedCoursesFromAllPotentialData(result, potentialData, contact, limit);
    }

    /// <summary>
    ///     Iterate every provided Faculty, get "best" source data among Faculty courses. At the same time, collect
    ///     all the rest of course data that wasn't used to try to create one <see cref="ContactLead" />.
    /// </summary>
    /// <param name="contact"></param>
    /// <param name="limit"></param>
    /// <param name="ct"></param>
    /// <returns>
    ///     Along Interested Courses created, return collected "the rest of" the data that will be analyzed next, if
    ///     result limit wasn't fulfilled.
    /// </returns>
    private async
        Task<(List<ContactLead> Result, Dictionary<TopSellerCoursesStatsDto, (Course, Faculty)> PotentialData)>
        CreateInterestedCoursesOverAllFaculties(Contact contact, int limit, CancellationToken ct)
    {
        var result = new List<ContactLead>();

        var potentialData = new Dictionary<TopSellerCoursesStatsDto, (Course, Faculty)>(contact.Faculties.Count * 10);

        foreach (var faculty in contact.Faculties)
        {
            var facultyTopSellers = await GetQualifiedFacultyTopSellerCourses(contact, faculty.Name, ct);
            if (facultyTopSellers is not { Count: > 0 })
            {
                continue;
            }

            var (stats, course) = SelectSourceDataByBestPricesAlgorithm(facultyTopSellers);

            CopyOtherThanBest(facultyTopSellers, stats, faculty);

            if (CovertToInterestedCourse(contact.Id, faculty.Id, stats, course) is { } interestedCourse)
            {
                result.Add(interestedCourse);
            }

            if (result.Count >= limit)
            {
                break;
            }
        }

        return (result, potentialData);

        void CopyOtherThanBest(
            Dictionary<TopSellerCoursesStatsDto, Course> facultyTopSellers,
            TopSellerCoursesStatsDto statsKey,
            Faculty faculty)
        {
            foreach (var pair in facultyTopSellers.Where(x => x.Key != statsKey))
            {
                potentialData.Add(pair.Key, (pair.Value, faculty));
            }
        }
    }

    /// <summary>
    ///     Retrieves statistics/input data from <see cref="ITopSellerCourseService" /> first, then excludes all courses
    ///     Contact already has in <see cref="Contact.ContactLeads" />.
    /// </summary>
    /// <param name="contact"></param>
    /// <param name="contactFacultyName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<Dictionary<TopSellerCoursesStatsDto, Course>?> GetQualifiedFacultyTopSellerCourses(
        Contact contact,
        string contactFacultyName,
        CancellationToken ct)
    {
        var cacheKey = new TopSellerCourseCacheKey(contactFacultyName, contact.CountryCode);
        var allFacultyTopSellerCourses = await _topSellersService.Get(cacheKey, ct);

        return allFacultyTopSellerCourses?
            .Where(
                kvp => !contact.ContactLeads.Exists(
                    cl => cl.CourseId == kvp.Value.Id || cl.CourseCode == kvp.Value.Code))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value);
    }

    /// <summary>
    ///     Takes "the rest of" the source course data and iterates without altering the order to try to fulfill the
    ///     amount of <paramref name="limit" /> of Interested Courses.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="potentialData"></param>
    /// <param name="contact"></param>
    /// <param name="limit"></param>
    /// <returns>"Pass through" list, that is modified as a result of current  method.</returns>
    private List<ContactLead> CreateInterestedCoursesFromAllPotentialData(
        List<ContactLead> result,
        Dictionary<TopSellerCoursesStatsDto, (Course, Faculty)> potentialData,
        Contact contact,
        int limit)
    {
        foreach (var (stats, (course, faculty)) in potentialData)
        {
            if (CovertToInterestedCourse(contact.Id, faculty.Id, stats, course) is { } interestedCourse)
            {
                result.Add(interestedCourse);
            }

            if (result.Count >= limit)
            {
                break;
            }
        }

        return result;
    }

    private ContactLead? CovertToInterestedCourse(
        int contactId,
        int facultyId,
        TopSellerCoursesStatsDto stats,
        Course course)
    {
        ContactLead? newEntity = null;
        try
        {
            var courseTypeName = course.CourseType?.Name ?? string.Empty;

            var courseData = course.CourseDataList.First(
                cd => cd.CourseCountry.Code
                    .Equals(stats.CourseCountryCode, StringComparison.InvariantCultureIgnoreCase));

            var courseCountry = courseData.CourseCountry;

            var courseFaculty = course.CourseFaculties.First(
                cf => cf.Faculty.Name
                    .Equals(stats.FacultyName, StringComparison.InvariantCultureIgnoreCase));

            newEntity = new()
            {
                Types = _contactLeadRecommendedType,
                ContactId = contactId,
                FacultyId = facultyId,
                CountryCode = stats.CourseCountryCode,
                CourseTypeId = course.CourseTypeId,
                CourseTypeName = courseTypeName,
                CourseId = course.Id,
                CourseCode = course.Code,
                Title = course.Title,
                AreaUrl = courseFaculty.Faculty.SeoUrl,
                Currency = courseCountry.CurrencyCode,
                CourseCountryId = courseCountry.Id,
                LanguageId = courseCountry.LanguageId,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // We won't stop job execution on single contact processing failure.
            _logger.LogWarning(
                ex,
                "Failed to instantiate `ContactLead` entity for given {ContactId} , but continue job execution",
                contactId);
        }

        return newEntity;
    }

    private static (TopSellerCoursesStatsDto Stats, Course Course) SelectSourceDataByBestPricesAlgorithm(
        IDictionary<TopSellerCoursesStatsDto, Course> facultyTopSellers)
    {
        var best = facultyTopSellers.MaxBy(kvp => kvp.Key.MaxPrice > 0 ? kvp.Key.MaxPrice : kvp.Key.MaxPriceFinal);
        var (stats, course) = best;

        return (stats, course);
    }

    private void AddInterestedCourses(List<ContactLead> newInterestedCourses, int processId) =>
        newInterestedCourses.ForEach(
            cl =>
            {
                _applicationDbContext.ContactLeadProcesses.Add(new() { ContactLead = cl, ProcessId = processId });
                _applicationDbContext.ContactLeads.Add(cl);
            });
}
