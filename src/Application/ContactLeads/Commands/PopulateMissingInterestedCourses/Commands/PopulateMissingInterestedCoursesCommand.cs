using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;
using CrmAPI.Application.Settings;
using CrmAPI.Contracts.Dtos;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Random = System.Random;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Commands;

/// <inheritdoc cref="PopulateMissingInterestedCoursesDto" />
public record PopulateMissingInterestedCoursesCommand(
    string Area,
    string CountryCode,
    string ApiKey,
    int? MaxJobContacts = null,
    int[]? ContactIds = null)
    : PopulateMissingInterestedCoursesDto(Area, CountryCode, MaxJobContacts, ContactIds),
        IHasApiKey,
        IRequest<ErrorOr<PopulateMissingInterestedCoursesResult>>;

[UsedImplicitly]
public class PopulateMissingInterestedCoursesHandler
    : IRequestHandler<PopulateMissingInterestedCoursesCommand, ErrorOr<PopulateMissingInterestedCoursesResult>>
{
    private readonly string _contactLeadRecommendedType;

    private readonly ITopSellerCourseService _topSellersService;

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly InterestedCoursePopulatorSettings _settings;
    private readonly ILogger<PopulateMissingInterestedCoursesHandler> _logger;

    private const string CreatorTag = "force-interest-course";
    private readonly PopulateMissingInterestedCoursesResult _resultStatistics;
    private readonly IContactLeadsAnalyzerService _contactLeadsAnalyzerService;

    public PopulateMissingInterestedCoursesHandler(
        IConfiguration configuration,
        IOptionsSnapshot<InterestedCoursePopulatorSettings> options,
        ITopSellerCourseService topSellersService,
        IContactLeadsAnalyzerService contactLeadsAnalyzerService,
        IApplicationDbContext applicationDbContext,
        ILogger<PopulateMissingInterestedCoursesHandler> logger)
    {
        _topSellersService = topSellersService;
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _contactLeadsAnalyzerService = contactLeadsAnalyzerService;
        _settings = options.Value;
        _contactLeadRecommendedType = configuration["Constants:ContactLeadRecommendedType"]!;

        _resultStatistics = new(_topSellersService.DateStart, _topSellersService.DateEnd);
    }

    /// <inheritdoc />
    public async Task<ErrorOr<PopulateMissingInterestedCoursesResult>> Handle(
        PopulateMissingInterestedCoursesCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting to handle the request {@Request}", request);

        // Takes care of excluded faculties and input validation.
        if (Validate(request) is { IsError: true } requestValidationResult)
        {
            _logger.LogError(
                "Command validation {ValidationResult}, job execution not started: {@Errors}",
                "failed",
                requestValidationResult.Errors);

            return requestValidationResult.Errors;
        }

        _logger.LogDebug("Command validation {ValidationResult}", "passed");

        // Based on request, detect workflow and pick the stream generator builder delegate.
        AnalyzerStreamBuilderDelegate streamBuilder = request.ContactIds is { Length: > 0 }
            ? _contactLeadsAnalyzerService.GetSpecificContactsStream
            : _contactLeadsAnalyzerService.GetAnyContactsStream;

        try
        {
            await ContactStreamIterator(request, streamBuilder, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            var error = Error.Unexpected("InterestedCourseCreator.OperationCancelled",
                metadata: GetErrorMetadata());

            _logger.LogWarning("Interested Course creation job stopped: {@Error}", error);

            return error;
        }
        catch (Exception ex)
        {
            _resultStatistics.SaveException(ex);

            var error = Error.Failure(
                description: "Failure during processing Interested courses population job.",
                metadata: GetErrorMetadata());

            _logger.LogCritical("Critical error encountered, execution cancelled: {@Error}", error);

            return error;
        }

        ReportNotFoundSpecificContacts(request, _resultStatistics);

        _logger.LogInformation("Interested Course creation job finished: {@Result}", _resultStatistics);

        return _resultStatistics;

        Dictionary<string, object> GetErrorMetadata()
        {
            ReportNotFoundSpecificContacts(request, _resultStatistics);

            return new() { { "Data collected so far", _resultStatistics } };
        }
    }

    /// <summary>
    ///     In case of <em>Specific Contacts</em> workflow, count the contacts that where found to be not processable.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="resultStatistics"></param>
    private static void ReportNotFoundSpecificContacts(
        PopulateMissingInterestedCoursesCommand request,
        PopulateMissingInterestedCoursesResult resultStatistics)
    {
        if (request.ContactIds is null or { Length: 0 })
        {
            // Nothing to report, not a Specific Contacts workflow.
            return;
        }

        var iteratedContactsCount = resultStatistics.TotalCoursesCreated + resultStatistics.TotalCoursesSkipped;

        if (request.ContactIds.Length - iteratedContactsCount is var remaining && remaining != 0)
        {
            resultStatistics.SetNotFoundContactsCount(remaining);
        }
    }

    /// <summary>
    ///     Validates request completely and returns all validation errors.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Validation errors in case of any validation failure.</returns>
    private ErrorOr<Success> Validate(PopulateMissingInterestedCoursesCommand request)
    {
        var errors = new List<Error>();

        var countryCode = request.CountryCode?.Trim() ?? string.Empty;
        if (!string.Equals(countryCode, PopulateMissingInterestedCoursesDto.Wildcard, StringComparison.InvariantCulture)
            && countryCode.Length < 2)
        {
            const string descriptionCountryCode = $"{nameof(request.CountryCode)} must be '{
                PopulateMissingInterestedCoursesDto.Wildcard
            }' or least 2 chars in length.";

            errors.Add(Error.Validation(nameof(request.CountryCode), descriptionCountryCode));
        }

        if (request.MaxJobContacts < PopulateMissingInterestedCoursesDto.Unlimited)
        {
            var descriptionMaxJob = $"{nameof(request.MaxJobContacts)} must be either `{
                PopulateMissingInterestedCoursesDto.Unlimited
            }` for unlimited or positive number of Contacts to process.";

            errors.Add(Error.Validation(nameof(request.MaxJobContacts), descriptionMaxJob));
        }

        if (request is { Area: PopulateMissingInterestedCoursesDto.Wildcard })
        {
            return errors.Count > 0
                ? errors
                : Result.Success;
        }

        if (_settings.ExcludedFaculties.Any(e => e.Equals(request.Area, StringComparison.InvariantCultureIgnoreCase)))
        {
            var descriptionArea = $"{nameof(request.Area)} must be '{
                PopulateMissingInterestedCoursesDto.Wildcard
            }' or not be one of '{string.Join(",", _settings.ExcludedFaculties)}'.";

            errors.Add(Error.Validation(nameof(request.Area), descriptionArea));
        }

        return errors.Count == 0
            ? Result.Success
            : errors;
    }

    private async Task ContactStreamIterator(
        PopulateMissingInterestedCoursesCommand request,
        AnalyzerStreamBuilderDelegate streamBuilder,
        CancellationToken ct)
    {
        var pagesProcessed = 0;

        using var _0 = _logger.BeginScope("ContactStreamIterator");

        _logger.LogDebug("Begin contacts stream consumption");

        var pagedContactsStream = streamBuilder.Invoke(request, ct)
            .ConfigureAwait(false);

        await foreach (var contactsPage in pagedContactsStream)
        {
            using var _1 = _logger.BeginScope(
                "page={CurrenContactPage}, size={CurrentContactPageSize}",
                ++pagesProcessed,
                contactsPage.Count);

            _logger.LogDebug("Begin processing a page");

            await ProcessContactsPage(_resultStatistics, contactsPage, ct);

            _logger.LogDebug("Finished processing a page");
        }

        _logger.LogDebug("Finished contacts stream consumption");
    }

    private async Task ProcessContactsPage(
        PopulateMissingInterestedCoursesResult resultStatistics,
        List<ContactFacultyDto> contactsPage,
        CancellationToken ct)
    {
        var facultyCountryContactGroups = contactsPage.GroupBy(
            dto => new TopSellerCourseCacheKey(dto.FacultyName, dto.ContactCountryCode));

        foreach (var topSellerGrouping in facultyCountryContactGroups)
        {
            var cacheKey = topSellerGrouping.Key;

            using var _ = _logger.BeginScope("Working with {@FacultyCourseData}", cacheKey);

            _logger.LogDebug("Begin TopSellers retrieval");

            var facultyCourseData = await _topSellersService.Get(cacheKey, ct)
                .ConfigureAwait(false);

            _logger.LogDebug("Finished TopSellers retrieval");

            // In Test environment it happened, not sure about PROD!
            if (facultyCourseData is null or { Count: 0 })
            {
                // Counting skipped Contacts!
                resultStatistics.UpdateOnSkipped(
                    cacheKey.FacultyName,
                    cacheKey.CountryCode,
                    topSellerGrouping.Count());

                _logger.LogDebug("No usable top sellers data, skip to next grouping");

                continue;
            }

            await SaveInterestedCourses(topSellerGrouping, facultyCourseData, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Creates <see cref="ContactLeads" /> entites, works through <paramref name="facultyContacts" /> chunking it
    ///     by <see cref=" InterestedCoursePopulatorSettings.EntityCreationMaxChunkSize" />, each chunk is projected
    ///     and saved to DB in implicit transaction.
    /// </summary>
    /// <param name="facultyContacts"></param>
    /// <param name="facultyTopSellers"></param>
    /// <param name="ct"></param>
    /// <remarks>
    ///     After saving each chunk, <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker" /> gets
    ///     cleared to avoid memory usage accumulation by entity changes data, that is not relevant anymore.
    /// </remarks>
    /// <returns>Count of created <see cref="ContactLeads" />.</returns>
    private async Task SaveInterestedCourses(
        IEnumerable<ContactFacultyDto> facultyContacts,
        IDictionary<TopSellerCoursesStatsDto, Course> facultyTopSellers,
        CancellationToken ct)
    {
        using var _0 = _logger.BeginScope(
            "SaveInterestedCourses operation, chunked by {EntityCreationMaxChunkSize}",
            _settings.EntityCreationMaxChunkSize);

        _logger.LogDebug("Begin saving a page of Interested Courses");

        foreach (var contactChunk in facultyContacts.Chunk(_settings.EntityCreationMaxChunkSize))
        {
            _logger.LogDebug("Create chunk of entities & save");

            var newEntities = contactChunk
                .Select(dto => ProjectToInterestedCourse(dto, facultyTopSellers))
                .Where(lead => lead is not null)
                .ToList();

            try
            {
                _applicationDbContext.ContactLeads.AddRange(newEntities!);

                // Implicit transaction 🚩.
                await _applicationDbContext.SaveChangesAsync(CreatorTag, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Problem saving a chunk of new entities, but continue to next chunk");

                continue;
            }

            CollectProgressCalculation(_resultStatistics, contactChunk);

            _logger.LogDebug("Clear UnitOfWork");
            _applicationDbContext.ClearUnitOfWork();
        }

        _logger.LogDebug("=> Finished saving a page of Interested Courses 🎉");
    }

    private ContactLead? ProjectToInterestedCourse(
        ContactFacultyDto contactDto,
        IDictionary<TopSellerCoursesStatsDto, Course> facultyTopSellers)
    {
        ContactLead? newEntity = null;
        try
        {
            var (stats, course) = facultyTopSellers.ElementAt(Random.Shared.Next(facultyTopSellers.Count));

            // TODO: Ensure that NullRefEx is not thrown here! Either guarantee required graph or assert here!

            var courseTypeName = course.CourseType?.Name ?? string.Empty;

            var courseData = course.CourseDataList.First(cd =>
                cd.CourseCountry.Code.Equals(stats.CourseCountryCode, StringComparison.InvariantCultureIgnoreCase));

            var courseCountry = courseData.CourseCountry;

            var courseFaculty = course.CourseFaculties.First(cf =>
                cf.Faculty.Name.Equals(stats.FacultyName, StringComparison.InvariantCultureIgnoreCase));

            newEntity = new()
            {
                Types = _contactLeadRecommendedType,
                ContactId = contactDto.ContactId,
                FacultyId = contactDto.FacultyId,
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
                 "Failed to instantiate `ContactLead` entity for Contact Id {ContactId}, but continue job execution",
                contactDto.ContactId);
        }

        return newEntity;
    }

    private static void CollectProgressCalculation(
        PopulateMissingInterestedCoursesResult resultStatistics,
        IEnumerable<ContactFacultyDto> interestedCourses)
    {
        var reportableGrouping = interestedCourses.GroupBy(
            cl => new { cl.FacultyName, cl.ContactCountryCode });

        foreach (var grouping in reportableGrouping)
        {
            resultStatistics.UpdateOnSuccess(
                grouping.Key.FacultyName,
                grouping.Key.ContactCountryCode,
                grouping.Count());
        }
    }
}
