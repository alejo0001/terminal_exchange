using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.Settings;
using CrmAPI.Contracts.Dtos;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;

/// <inheritdoc />
[UsedImplicitly]
public class ContactLeadsAnalyzerService : IContactLeadsAnalyzerService
{
    private static readonly TimeSpan LongRunningCommandTimeout;

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly InterestedCoursePopulatorSettings _settings;
    private readonly ILogger<ContactLeadsAnalyzerService> _logger;

    public ContactLeadsAnalyzerService(
        IApplicationDbContext applicationDbContext,
        IOptionsSnapshot<InterestedCoursePopulatorSettings> options,
        ILogger<ContactLeadsAnalyzerService> logger)
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _settings = options.Value;
    }

    static ContactLeadsAnalyzerService() => LongRunningCommandTimeout = TimeSpan.FromMinutes(2);

    /// <inheritdoc />
    public async IAsyncEnumerable<List<ContactFacultyDto>> GetSpecificContactsStream(
        PopulateMissingInterestedCoursesDto requestDto,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Short-circuit!
        if (requestDto.ContactIds is null or { Length: 0 })
        {
            yield break;
        }

        // Clean & groom the input a bit. We will relie on the order of ids later.
        var allFixedContactIds = requestDto.ContactIds
            .Distinct()
            .Order()
            .ToArray();

        var pageSize = Math.Clamp(allFixedContactIds.Length, 0, _settings.ContactsQueryMaxPageSize);

        var queriedSoFar = 0;

        var requestClone = CloneRequestForNextPage(
            requestDto,
            allFixedContactIds,
            queriedSoFar,
            pageSize);

        while (await MaterializeSpecificStreamPage(requestClone, ct).ConfigureAwait(false) is { Count: > 0 } list)
        {
            yield return list;

            queriedSoFar += requestClone.ContactIds!.Length;

            if (queriedSoFar >= allFixedContactIds.Length)
            {
                yield break;
            }

            requestClone = CloneRequestForNextPage(
                requestClone,
                allFixedContactIds,
                queriedSoFar,
                pageSize);
        }
    }

    /// <summary>
    ///     Clones request record: takes Contact IDs for next page query execution.
    /// </summary>
    /// <param name="requestDto">
    ///     Order of <see cref="PopulateMissingInterestedCoursesDto.ContactIds" /> is important!
    /// </param>
    /// <param name="allContactIds"></param>
    /// <param name="queriedSoFar"></param>
    /// <param name="pageSize">
    ///     This will be used to remove those Contact IDs from the beginning of IDs array, for next
    ///     query execution.
    /// </param>
    /// <returns>Cloned immutable DTO.</returns>
    private static PopulateMissingInterestedCoursesDto CloneRequestForNextPage(
        PopulateMissingInterestedCoursesDto requestDto,
        int[] allContactIds,
        int queriedSoFar,
        int pageSize)
    {
        pageSize = GetNextPageSize(pageSize, allContactIds.Length, queriedSoFar);

        var contactIds = allContactIds
            .Skip(queriedSoFar)
            .Take(pageSize)
            .ToArray();

        return requestDto with { ContactIds = contactIds };
    }

    /// <summary>
    ///     Builds query for "Specific Contacts" workflow and starts its materialization task.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns>Task to get materialized list.</returns>
    private Task<List<ContactFacultyDto>> MaterializeSpecificStreamPage(
        PopulateMissingInterestedCoursesDto request,
        CancellationToken ct)
    {
        var contactsMainQuery = BuildContactsMainQuery(request);
        var queryToMaterialize = BuildFinalQuery(contactsMainQuery);

        return ExecuteQueryAsLongRunning(queryToMaterialize, ct);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<List<ContactFacultyDto>> GetAnyContactsStream(
        PopulateMissingInterestedCoursesDto requestDto,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Short-circuit!
        if (requestDto.MaxJobContacts.GetValueOrDefault() == 0)
        {
            yield break;
        }

        var maxJobSize = requestDto.MaxJobContacts!.Value;

        // Idea is to continue querying up to maximum job size of contacts or till query won't yield more contacts.
        var pageSize = maxJobSize == PopulateMissingInterestedCoursesDto.Unlimited
            ? _settings.ContactsQueryMaxPageSize
            : Math.Clamp(maxJobSize, 0, _settings.ContactsQueryMaxPageSize);

        var retrievedSoFar = 0;
        var lastHighestContactId = 0;

        while (await MaterializeAnyStreamPage(requestDto, pageSize, lastHighestContactId, ct).ConfigureAwait(false)
               is { Count: > 0 } list)
        {
            yield return list;

            lastHighestContactId = list.Last().ContactId;

            if (maxJobSize == PopulateMissingInterestedCoursesDto.Unlimited)
            {
                continue;
            }

            retrievedSoFar += list.Count;

            if (retrievedSoFar >= maxJobSize)
            {
                yield break;
            }

            pageSize = GetNextPageSize(pageSize, maxJobSize, retrievedSoFar);
        }
    }

    /// <summary>
    ///     Builds query for "Any Contacts" workflow and starts its materialization task.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="pageSize"></param>
    /// <param name="lastHighestContactId">
    ///     Query will be prepended by criterion that already processed Contacts won't be processed anymore.
    /// </param>
    /// <param name="ct"></param>
    /// <returns>Task to get materialized list.</returns>
    private Task<List<ContactFacultyDto>> MaterializeAnyStreamPage(
        PopulateMissingInterestedCoursesDto request,
        int pageSize,
        int lastHighestContactId,
        CancellationToken ct)
    {
        var contactsMainQuery = BuildContactsMainQuery(request, lastHighestContactId);
        var queryToMaterialize = BuildFinalQuery(contactsMainQuery, pageSize);

        return ExecuteQueryAsLongRunning(queryToMaterialize, ct);
    }

    /// <summary>
    ///     Applies main contacts filtering logic. Considers workflow and applies conditionally Faculty name and
    ///     Country Code filtrations. Query composition don't consider any paging logic,
    ///     it must be done outside, when desirable.
    /// </summary>
    /// <remarks>Respects Soft-Deleted pattern.</remarks>
    /// <returns>A complete query that has business logic and workflow filtering applied.</returns>
    private IQueryable<ContactFacultyDto> BuildContactsMainQuery(
        PopulateMissingInterestedCoursesDto request,
        int? lastHighestContactId = null)
    {
        // We keep this step first, because it can produce more strict filter early, therefor execution will be faster.
        var (query, skipOtherFilters) = ApplyIdsFilter(request.ContactIds, lastHighestContactId);

        // Common filtering criteria, that must always be present, but order matters.
        query = query.Where(c => !c.IsDeleted);

        query = ApplyMissingInterestedCourseFilter(query);

        query = ApplyCountryFilter(query, request.CountryCode, skipOtherFilters);

        var intermediateQuery = ApplyFacultyFilter(query, request.Area, skipOtherFilters);

        var projectedQuery = intermediateQuery
            .Select<IntermediateProjectionDto, ContactFacultyDto>(dto => new()
            {
                ContactId = dto.ContactId,
                FacultyId = dto.FirstFaculty.Id,
                FacultyName = dto.FirstFaculty.Name.ToLower(),
                ContactCountryCode = dto.ContactCountryCode.ToUpper(),
            });

        return projectedQuery;
    }

    /// <summary>
    ///     It will detect workflow type (Specific or Any) and sets ContactId filtering accordingly.
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="lastHighestContactId"></param>
    /// <returns>
    ///     A tuple of query and workflow flag as skipOther: <c>true</c> is "Specific", <c>false</c> is "Any".
    /// </returns>
    private (IQueryable<Contact> query, bool skipOthers) ApplyIdsFilter(int[]? ids, int? lastHighestContactId)
    {
        IQueryable<Contact> query = _applicationDbContext.Contact;
        if (lastHighestContactId is > 0)
        {
            query = _applicationDbContext.Contact.Where(c => c.Id > lastHighestContactId); // Any
        }

        return ids is { Length: > 0 }
            ? (query.Where(c => ids.Contains(c.Id)), true) // Specific
            : (query, false); // Any
    }

    /// <summary>
    ///     Add criteria to detect Contacts that don't have <em>Interested Course</em>.
    /// </summary>
    /// <remarks>
    ///     Definition on "Interested Course": if Contact has ContactLead with either `CourseId` or `CourseCode`.
    /// </remarks>
    /// <param name="query"></param>
    /// <returns></returns>
    private static IQueryable<Contact> ApplyMissingInterestedCourseFilter(IQueryable<Contact> query)
    {
        // Details: even though it is rare that only one of them has value, it is requested from Business, that this
        // check exists. So, we prioritize checking `CourseId` because it is FK; lack of it will cause to check
        // `CourseCode` as a last resort.
        return query.Where(c =>
            c.ContactLeads.All(cl => cl.IsDeleted)
            || c.ContactLeads.All(cl => cl.CourseId == null || string.IsNullOrWhiteSpace(cl.CourseCode)));
    }

    /// <param name="query"></param>
    /// <param name="countryCode">
    ///     If equals to <see cref="PopulateMissingInterestedCoursesDto.Wildcard" />,
    ///     then will not filter by country code.
    /// </param>
    /// <param name="skip">
    ///     If <c>true</c> then Country Code filtering is skipped, otherwise provided Country Code value will be
    ///     analyzed next.
    /// </param>
    private static IQueryable<Contact> ApplyCountryFilter(IQueryable<Contact> query, string countryCode, bool skip) =>
        skip || countryCode == PopulateMissingInterestedCoursesDto.Wildcard
            ? query.Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
            : query.Where(c => c.CountryCode.ToUpper() == countryCode.ToUpper());

    /// <summary>Applies Faculty filtering conditionally.</summary>
    /// <param name="query"></param>
    /// <param name="facultyName">
    ///     If equals to <see cref="PopulateMissingInterestedCoursesDto.Wildcard" />,
    ///     then will filter Contacts that have any Faculty present, otherwise by provided Faculty name.
    /// </param>
    /// <param name="skipFacultyCriteria">
    ///     If <c>true</c> then existing first Contact's faculty will be taken, otherwise provided faculty criteria will
    ///     be added.
    /// </param>
    private IQueryable<IntermediateProjectionDto> ApplyFacultyFilter(
        IQueryable<Contact> query,
        string facultyName,
        bool skipFacultyCriteria) =>
        skipFacultyCriteria || facultyName == PopulateMissingInterestedCoursesDto.Wildcard
            ? ApplyAnyWorkflowFacultyFiltration(query)
            : ApplySpecificWorkflowFacultyFiltration(query, facultyName);

    /// <remarks>
    ///     Exclusions defined in <see cref="InterestedCoursePopulatorSettings.ExcludedFaculties" />
    ///     will be respected.
    /// </remarks>
    private IQueryable<IntermediateProjectionDto> ApplySpecificWorkflowFacultyFiltration(
        IQueryable<Contact> query,
        string facultyName) => query
        .Where(c => c.Faculties.Any(f =>
            !f.IsDeleted
            && !_settings.ExcludedFaculties.Contains(f.Name.ToUpper())
            && f.Name.ToUpper() == facultyName.ToUpper()))
        .Select(c => new IntermediateProjectionDto
        {
            ContactId = c.Id,
            ContactCountryCode = c.CountryCode,
            FirstFaculty = c.Faculties.First(f =>
                !f.IsDeleted
                && !_settings.ExcludedFaculties.Contains(f.Name.ToUpper())
                && f.Name.ToUpper() == facultyName.ToUpper()),
        });

    /// <remarks>
    ///     Exclusions defined in <see cref="InterestedCoursePopulatorSettings.ExcludedFaculties" />
    ///     will be respected.
    /// </remarks>
    private IQueryable<IntermediateProjectionDto> ApplyAnyWorkflowFacultyFiltration(IQueryable<Contact> query) => query
        .Where(c => c.Faculties.Any(f =>
            !f.IsDeleted
            && !_settings.ExcludedFaculties.Contains(f.Name.ToUpper())))
        .Select(c => new IntermediateProjectionDto
        {
            ContactId = c.Id,
            ContactCountryCode = c.CountryCode,
            FirstFaculty = c.Faculties.First(f =>
                !f.IsDeleted
                && !_settings.ExcludedFaculties.Contains(f.Name.ToUpper())),
        });

    /// <summary>
    ///     Creates final query, that is sorted by <see cref="ContactFacultyDto.ContactId" /> and limited by page size.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pageSize">
    ///     Optional. Be user that it is omitted only for "Specific Contacts" workflow, otherwise it can can
    ///     cause performance hit by querying unknown amount of results, which defeats the purpose of stream logic.
    /// </param>
    /// <returns>Final query ready to be materialized be EF Core.</returns>
    /// <remarks>
    ///     NB! Skip is not desirable, because in case we are working on "stream" of contacts, and they are retrieved
    ///     page-x-page from DB, new Interested Courses are added. It means that every execution of this query will
    ///     not hit those contacts anymore. Skip on the other hand would cause to omit some of the contacts
    ///     by the end of single execution of this job, that are still missing Interested Courses.<br />
    ///     This service's internal consumer is relying on Ordering of Contact ID is
    /// </remarks>
    private static IQueryable<ContactFacultyDto> BuildFinalQuery(
        IQueryable<ContactFacultyDto> query,
        int? pageSize = null)
    {
        query = query.AsNoTracking()
            .AsSplitQuery()
            .OrderBy(x => x.ContactId);

        if (pageSize is not null)
        {
            query = query.Take(pageSize.Value);
        }

        return query;
    }

    /// <summary>
    ///     Purpose is to set and restore "command timeout" just for this execution as a long-running execution.
    /// </summary>
    /// <returns>Materialized list.</returns>
    private async Task<List<ContactFacultyDto>> ExecuteQueryAsLongRunning(
        IQueryable<ContactFacultyDto> queryToMaterialize,
        CancellationToken ct)
    {
        var originalTimeout = _applicationDbContext.Database.GetCommandTimeout();
        _applicationDbContext.Database.SetCommandTimeout(LongRunningCommandTimeout); // Set a long timeout

        _logger.LogDebug("Begin a contacts page retrieval");

        List<ContactFacultyDto> result;
        try
        {
            result = await queryToMaterialize.ToListAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _applicationDbContext.Database.SetCommandTimeout(originalTimeout); // Reset to the original timeout
        }

        _logger.LogDebug("Finish a contacts page retrieval");

        return result;
    }

    /// <summary>
    ///     Checks whether next page is smaller than current, to detect approaching stream end.
    /// </summary>
    /// <param name="currentPageSize"></param>
    /// <param name="targetSize"></param>
    /// <param name="queriedSoFar"></param>
    /// <returns>
    ///     If remaining items count is less than <paramref name="currentPageSize" />,
    ///     than remaining will return; otherwise <paramref name="currentPageSize" />.
    /// </returns>
    private static int GetNextPageSize(int currentPageSize, int targetSize, int queriedSoFar) =>
        targetSize - queriedSoFar is var remaining && remaining < currentPageSize
            ? remaining
            : currentPageSize;
}
