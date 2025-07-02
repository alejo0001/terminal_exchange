using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Commands.ConsolidateContactsCommand;
using CrmAPI.Domain.Leads.Entities;
using ErrorOr;
using Humanizer;
using IntranetMigrator.Domain.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Contacts.Services;

/// <inheritdoc />
public class ContactsConsolidatorService : IContactsConsolidatorService
{
    public const string LeadTagFusionFormat = "fusion_duplicados_emails_{0:yyyy-MM-dd}";
    private static readonly TimeSpan LongRunningCommandTimeout;

    private readonly IApplicationDbContext _contextIntranet;
    private readonly IEntityClonerService<IApplicationDbContext> _entityClonerService;
    private readonly ILeadsDbContext _contextLeads;
    private readonly IEFCoreFunctions _efCoreFunctions;
    private readonly ILogger<ContactsConsolidatorService> _logger;

    public ContactsConsolidatorService(
        IApplicationDbContext contextIntranet,
        IEntityClonerService<IApplicationDbContext> entityClonerService,
        ILeadsDbContext contextLeads,
        IEFCoreFunctions efCoreFunctions, 
        ILogger<ContactsConsolidatorService> logger)
    {
        _contextIntranet = contextIntranet;
        _entityClonerService = entityClonerService;
        _contextLeads = contextLeads;
        _efCoreFunctions = efCoreFunctions;
        _logger = logger;
    }
    
    static ContactsConsolidatorService() => LongRunningCommandTimeout = TimeSpan.FromMinutes(5);


    /// <inheritdoc />
    public async Task<ErrorOr<Success>> ConsolidateContacts(
        int originContactId,
        int destinationContactId,
        CancellationToken ct)
    {
        // It will carry already EF Tracked entities down the flow to "upsert" entities.
        if (await GetOriginContactDto(originContactId, ct) is not { } originIntranetContactDto)
        {
            return GetOriginContactNotFoundError(originContactId);
        }

        // Extract before modifying origin entities, because filtration can get messed up regarding requirements.
        var allOriginEmails = GetOriginEmails(originIntranetContactDto);

        SetSoftDeleteOriginContactAndDesignatedRelatedEntities(originIntranetContactDto);

        UpdateDesignatedRelatedEntitiesToDestinationContact(originIntranetContactDto, destinationContactId);

        DeepCopyDesignatedRelatedEntitiesToDestinationContact(originIntranetContactDto, destinationContactId);

        await DeepCopySpecialCaseContactLeadToDestinationContact(originIntranetContactDto, destinationContactId, ct);

        if (await GetLeadsOfOriginContact(allOriginEmails, ct) is not { Count: > 0 } originLeads)
        {
            return GetOriginLeadsNotFoundError();
        }

        SetFusedTagForOriginLeads(originLeads);

        return Result.Success;
    }

    private async Task<OriginContactDto?> GetOriginContactDto(int originContactId, CancellationToken ct)
    {
        var query = _contextIntranet.Contact
            .Where(c => c.Id == originContactId)
            .Where(c => !c.IsDeleted);

        var fullProjectionWithSubqueries = query.Select(c => new OriginContactDto
        {
            Contact = c,
            Processes = _contextIntranet.Processes
                .Where(p => p.ContactId == c.Id)
                .Where(p => !p.IsDeleted)
                .ToList(),
            Actions = _contextIntranet.Actions
                .Where(ac => ac.ContactId == c.Id)
                .Where(ac => !ac.IsDeleted)
                .ToList(),
            Annotations = _contextIntranet.Annotations
                .Where(an => an.ContactId == c.Id)
                .Where(an => !an.IsDeleted)
                .ToList(),
            OrdersImported = _contextIntranet.OrdersImported
                .Where(oi => oi.ContactId == c.Id)
                .Where(oi => !oi.IsDeleted)
                .ToList(),
            ContactEmails = _contextIntranet.ContactEmail
                .Where(ce => ce.ContactId == c.Id)
                .Where(ce => !ce.IsDeleted)
                .ToList(),
            ContactPhones = _contextIntranet.ContactPhone
                .Where(cp => cp.ContactId == c.Id)
                .Where(cp => !cp.IsDeleted)
                .ToList(),
            ContactLeads = _contextIntranet.ContactLeads
                .Where(cl => cl.ContactId == c.Id)
                .Where(cl => !cl.IsDeleted)
                .ToList(),
            ContactAddresses = _contextIntranet.ContactAddress
                .Where(ca => ca.ContactId == c.Id)
                .Where(ca => !ca.IsDeleted)
                .ToList(),
            ContactLeadProcesses = _contextIntranet.ContactLeadProcesses
                .Where(clp => clp.ContactLead.ContactId == c.Id)
                .Where(clp => !clp.IsDeleted)
                .ToList(),
            ContactTitles = _contextIntranet.ContactTitles
                .Where(cti => cti.ContactId == c.Id)
                .Where(cti => !cti.IsDeleted)
                .ToList(),
            ContactSpecialities = _contextIntranet.ContactSpeciality
                .Where(cs => cs.ContactId == c.Id)
                .Where(cs => !cs.IsDeleted)
                .ToList(),
            ContactFaculties = _contextIntranet.ContactFaculty
                .Where(cf => cf.ContactId == c.Id)
                .Where(cf => !cf.IsDeleted)
                .ToList(),
            ContactLanguages = _contextIntranet.ContactLanguages
                .Where(cl => cl.ContactId == c.Id)
                .Where(cl => !cl.IsDeleted)
                .ToList(),
        });

        return await fullProjectionWithSubqueries.AsSplitQuery().FirstOrDefaultAsync(ct);
    }

    private static List<string> GetOriginEmails(OriginContactDto originIntranetContactDto) =>
        originIntranetContactDto.ContactEmails
            .Where(c => !string.IsNullOrWhiteSpace(c.Email))
            .Select(x => x.Email)
            .Distinct()
            .ToList();

    /// <summary>
    ///     Soft-deletes designated related entities of the Origin contact:
    ///     <see cref="ContactLead" />,
    ///     <see cref="ContactTitle" />,
    ///     <see cref="ContactEmail" />,
    ///     <see cref="ContactPhone" />,
    ///     <see cref="ContactAddress" />,
    ///     <see cref="ContactLanguage" />,
    ///     <see cref="ContactFaculty" />,
    ///     <see cref="ContactSpeciality" />
    ///     and <see cref="ContactLeadProcess" />.
    /// </summary>
    /// <param name="originContactDto">DTO with data</param>
    /// <returns></returns>
    private static void SetSoftDeleteOriginContactAndDesignatedRelatedEntities(OriginContactDto originContactDto)
    {
        originContactDto.Contact.IsDeleted = true;

        originContactDto.ContactLeads.ForEach(cl => cl.IsDeleted = true);
        originContactDto.ContactTitles.ForEach(ct => ct.IsDeleted = true);
        originContactDto.ContactEmails.ForEach(ce => ce.IsDeleted = true);
        originContactDto.ContactPhones.ForEach(cp => cp.IsDeleted = true);
        originContactDto.ContactAddresses.ForEach(ca => ca.IsDeleted = true);
        originContactDto.ContactLanguages.ForEach(cn => cn.IsDeleted = true);
        originContactDto.ContactFaculties.ForEach(cf => cf.IsDeleted = true);
        originContactDto.ContactSpecialities.ForEach(cs => cs.IsDeleted = true);
        originContactDto.ContactLeadProcesses.ForEach(cp => cp.IsDeleted = true);
    }

    /// <summary>
    ///     Sets `ContactId` foreign relation to the Destination Contact ID for
    ///     <see cref="Process" />, <see cref="IntranetMigrator.Domain.Entities.Action" />, <see cref="Annotation" />
    ///     and <see cref="OrdersImported" /> entities.
    /// </summary>
    /// <param name="originContactDto"></param>
    /// <param name="destinationContactId"></param>
    private static void UpdateDesignatedRelatedEntitiesToDestinationContact(
        OriginContactDto originContactDto,
        int destinationContactId)
    {
        originContactDto.Actions.ForEach(ac => ac.ContactId = destinationContactId);
        originContactDto.Processes.ForEach(p => p.ContactId = destinationContactId);
        originContactDto.Annotations.ForEach(an => an.ContactId = destinationContactId);
        originContactDto.OrdersImported.ForEach(o => o.ContactId = destinationContactId);
    }

    private void DeepCopyDesignatedRelatedEntitiesToDestinationContact(
        OriginContactDto originContactDto,
        int destinationContactId)
    {
        foreach (var newEntity in
                 originContactDto.ContactEmails.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
            newEntity.IsDefault = null;
        }

        foreach (var newEntity in
                 originContactDto.ContactPhones.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
            newEntity.IsDefault = null;
        }

        foreach (var newEntity in
                 originContactDto.ContactAddresses.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
            newEntity.IsDefault = null;
        }

        foreach (var newEntity in
                 originContactDto.ContactLanguages.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
            newEntity.IsDefault = false;
        }

        foreach (var newEntity in
                 originContactDto.ContactTitles.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
        }

        foreach (var newEntity in
                 originContactDto.ContactFaculties.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
        }

        foreach (var newEntity in
                 originContactDto.ContactSpecialities.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
        }
    }

    /// <summary>
    ///     <see cref="ContactLead.CourseId" /> cannot be duplicated in terms of existing course. Only copy when destination
    ///     don't have <see cref="IntranetMigrator" /> with same existing course.<br />
    ///     <i>
    ///         Def. existing course: <br />
    ///         * both contacts have matching course, when <see cref="IntranetMigrator.Domain" /> or
    ///         <see cref="IntranetMigrator.Domain" /> are equal;<br />
    ///         * if both <see cref="IntranetMigrator.Domain" /> or <see cref="ContactLead" /> are <c>null</c>, then
    ///         this is not considers as a "course lead" at all.
    ///     </i>
    /// </summary>
    /// <param name="originContactDto"></param>
    /// <param name="destinationContactId"></param>
    /// <param name="ct"></param>
    private async Task DeepCopySpecialCaseContactLeadToDestinationContact(
        OriginContactDto originContactDto,
        int destinationContactId,
        CancellationToken ct)
    {
        var matchingContactLeads = await GetDestinationContactMatchingContactLeads(
            originContactDto,
            destinationContactId,
            ct);

        var contactLeadsToCopy = GetContactLeadsToCopy(originContactDto, matchingContactLeads);

        foreach (var newEntity in
                 contactLeadsToCopy.Select(_entityClonerService.DeepCopyAndBaseEntityReset))
        {
            newEntity.ContactId = destinationContactId;
        }
    }

    private async Task<List<(int? CourseId, string CourseCode)>> GetDestinationContactMatchingContactLeads(
        OriginContactDto originContactDto,
        int destinationContactId,
        CancellationToken ct)
    {
        var destinationLeads = await _contextIntranet.ContactLeads
            .Where(cl => cl.ContactId == destinationContactId)
            .Where(cl => !cl.IsDeleted)
            .Where(cl => cl.CourseId != null)
            .Where(cl => !string.IsNullOrWhiteSpace(cl.CourseCode))
            .Select(cl => new { cl.CourseId, cl.CourseCode })
            .ToListAsync(ct);

        var originLeadCourseDescriptors = originContactDto.ContactLeads
            .Select(cl => (cl.CourseId, cl.CourseCode));

        var matchingResult = destinationLeads
            .Where(cl => originLeadCourseDescriptors.Any(x =>
                x.CourseId.Equals(cl.CourseId)
                || x.CourseCode.Equals(cl.CourseCode)))
            .Select(x => (x.CourseId, x.CourseCode))
            .ToList();

        return matchingResult;
    }

    /// <summary>
    ///     Avoid duplication of <see cref="originContactDto" />s, get only those, that Destination Contact doesn't have yet.
    /// </summary>
    /// <param name="originContactDto"></param>
    /// <param name="matchingContactLeads"></param>
    /// <returns></returns>
    private static IEnumerable<ContactLead> GetContactLeadsToCopy(
        OriginContactDto originContactDto,
        List<(int? CourseId, string CourseCode)> matchingContactLeads) =>
        originContactDto.ContactLeads.SkipWhile(cl =>
            matchingContactLeads.Any(x =>
                x.CourseId.Equals(cl.CourseId)
                || x.CourseCode.Equals(cl.CourseCode)));

    /// <summary>
    ///     Gets <see cref="Lead" /> entities that are in
    ///     <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker" />.
    /// </summary>
    /// <remarks>
    ///     NB! Make sure that <paramref name="originContactEmails" /> is not empty nor contain null/empty elements,
    ///     because DB is queried using <c>LIKE</c> operator and empty pattern has potential to return unknown amount
    ///     of rows.
    /// </remarks>
    /// <param name="originContactEmails">Clean array, without null/emty elements.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<List<Lead>> GetLeadsOfOriginContact(List<string> originContactEmails, CancellationToken ct)
    {
        // Though use case logic won't reach here if there are no emails, we still avoid bugs at all cost.
        if (originContactEmails is not { Count: > 0 })
        {
            return new();
        }

        var likePatterns = originContactEmails
            .Select(e => $"%{e}%")
            .ToArray();

        var emailPredicate = _efCoreFunctions.LikeOr<Lead>(l => l.email!, likePatterns);

        var queryable = _contextLeads.Leads.AsExpandable()
            .Where(emailPredicate);
        
        return await ExecuteQueryAsLongRunning(queryable, ct);
    }
    
    private async Task<List<Lead>> ExecuteQueryAsLongRunning(
        IQueryable<Lead> queryToMaterialize,
        CancellationToken ct)
    {
        var originalTimeout = _contextLeads.Database.GetCommandTimeout();
        _contextLeads.Database.SetCommandTimeout(LongRunningCommandTimeout); // Set a long timeout

        List<Lead> result;
        try
        {
            result = await queryToMaterialize.ToListAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _contextLeads.Database.SetCommandTimeout(originalTimeout); // Reset to the original timeout
        }

        _logger.LogDebug("Finish a contacts page retrieval");

        return result;
    }

    private static void SetFusedTagForOriginLeads(List<Lead> originLeads)
    {
        var tagValueToAppend = string.Format(LeadTagFusionFormat, DateTime.UtcNow);

        originLeads.ForEach(lead =>
        {
            if (lead.tags?.Contains(tagValueToAppend, StringComparison.InvariantCulture) ?? false)
            {
                return;
            }

            lead.tags = new StringBuilder(lead.tags).AppendJoin(", ", tagValueToAppend).ToString();
        });
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SaveChanges(CancellationToken ct)
    {
        await using var intranetTransaction = await _contextIntranet.Database.BeginTransactionAsync(ct);
        await using var leadsTransaction = await _contextLeads.Database.BeginTransactionAsync(ct);

        var errors = new List<Error>();

        try
        {
            await _contextIntranet.SaveChangesAsync(ct);
            await _contextLeads.SaveChangesAsync(ct);

            await intranetTransaction.CommitAsync(ct);
            await leadsTransaction.CommitAsync(ct);
        }
        catch (Exception saveException)
        {
            errors.Add(GetSavingChangesError(saveException));

#pragma warning disable CA2016
            try
            {
                // ReSharper disable once MethodSupportsCancellation
                await intranetTransaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                errors.Add(GetRollbackError("intranet", rollbackEx));
            }

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                await leadsTransaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                errors.Add(GetRollbackError("leads", rollbackEx));
            }
#pragma warning restore CA2016

            return errors;
        }

        return default;
    }

    private static Error GetOriginContactNotFoundError(int originContactId) =>
        Error.Custom(
            IContactsConsolidatorService.OriginContactNotFoundError,
            $"{nameof(ContactsConsolidatorService)}.{IContactsConsolidatorService.OriginContactNotFoundError}",
            "Origin contact not found.",
            new() { { nameof(originContactId), originContactId } });

    private static Error GetOriginLeadsNotFoundError() =>
        Error.Custom(
            IContactsConsolidatorService.OriginLeadsNotFoundError,
            $"{nameof(ContactsConsolidatorService)}.{IContactsConsolidatorService.OriginLeadsNotFoundError}",
            "Origin Leads not found in Leads/Potenciales database.");

    private static Error GetSavingChangesError(Exception saveException) => Error.Failure(
        $"{nameof(ContactsConsolidatorService)}.ChangesNotSaved",
        "Error encountered while saving changes. Transaction(s) rolled back. See exception for details.",
        new() { { "Exception", saveException } });

    private static Error GetRollbackError(string dbName, Exception rollbackEx) => Error.Unexpected(
        $"{nameof(ContactsConsolidatorService)}.RollbackSavedChangeFailure",
        $"{dbName.Titleize()} transaction rollback failure. See exception for details.",
        new() { { "Exception", rollbackEx } });
}
