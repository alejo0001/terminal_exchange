using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.BusinessAlgorithms;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Emails.Services;

/// <inheritdoc />
public class CrmMailerService : ICrmMailerService
{
    #region Dependencies

    private const string EntityCreatorTag = nameof(CrmMailerService);
    private const int TagIdCrmInternational = 12;
    private const int DefaultLanguageIdEs = 1;
    private readonly string _auditEmailAddress;

    private readonly string _containerSignatureName;
    private readonly string _connectionStringSignature;
    private readonly string _techDefaultFromAddress;
    private readonly decimal _scholarshipDiscountPercentFallback;

    private const string LogErrMissingCoursesPrices = "Missing one or more courses' prices; job halted";

    private static readonly Action<Email, Action> NoOpConfigureEntitiesDlgt;

    private readonly ConcurrentDictionary<TemplateCacheKey, Template?> _templateCache;

    private readonly IApplicationDbContext _dbContext;
    private readonly IManagementApiClient _managementApiClient;
    private readonly IEmailSendTestingFeatureFlagService _emailSendTestingFeatureFlagService;
    private readonly IBus _bus;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<CrmMailerService> _logger;

    #endregion

    static CrmMailerService()
    {
        NoOpConfigureEntitiesDlgt = (_, _) => { };
    }

    public CrmMailerService(
        IApplicationDbContext dbContext,
        IBus bus,
        IConfiguration configuration,
        IBlobStorageService blobStorageService,
        IManagementApiClient managementApiClient,
        IEmailSendTestingFeatureFlagService emailSendTestingFeatureFlagService,
        ILogger<CrmMailerService> logger)
    {
        _dbContext = dbContext;
        _bus = bus;
        _logger = logger;
        _managementApiClient = managementApiClient;
        _emailSendTestingFeatureFlagService = emailSendTestingFeatureFlagService;
        _blobStorageService = blobStorageService;

        _techDefaultFromAddress = configuration["Constants:DefaultAddressSendEmailNewCouponProcess"]
                                  ?? throw new InvalidOperationException(
                                      "Missing config `Constants:DefaultAddressSendEmailNewCouponProcess`");

        _auditEmailAddress = configuration["Constants:AuditEmailAddress"]
                             ?? throw new InvalidOperationException("Missing config `Constants:AuditEmailAddress`");

        _containerSignatureName = configuration["BlobSignatures:ContainerName"]
                                  ?? throw new InvalidOperationException(
                                      "Missing config `BlobSignatures:ContainerName`");

        _connectionStringSignature = configuration["BlobSignatures:ConnectionString"]
                                     ?? throw new InvalidOperationException(
                                         "Missing config `BlobSignatures:ConnectionString`");
        _templateCache = new();

        _scholarshipDiscountPercentFallback = configuration.GetValue<decimal>(
            "Constants:ScholarshipDiscountPercentFallback");
    }

    public Task<CreateEmail> BuildEmail(
        Process process,
        string receiver,
        string? fromEmail,
        string subject,
        string body,
        CancellationToken ct)
    {
        
        var receivers = new List<string> { receiver };
        var from = EmailHeadersHelper.GetFrom(process, fromEmail, _techDefaultFromAddress);
        var fromName = EmailHeadersHelper.GetFromName(process);
        var isTestEmail = _emailSendTestingFeatureFlagService.IsAutomaticEmailSendingForcedToTestOnly;

        var emailContract = new CreateEmail
        {
            CorrelationId = NewId.NextSequentialGuid(),
            Subject = subject,
            From = from,
            Body = body,
            FromName = fromName,
            Receivers = receivers,
            BccReceivers = new() { _auditEmailAddress },
            UseExchange = false,
            IsTestEmail = isTestEmail,
        };
        return Task.FromResult(emailContract);
    }

    #region Get Process Query

    public async Task<Process> GetProcessesWithRelatedData(
        int processId,
        CancellationToken ct)
    {
        var completeQuery = GetProcessQueryCore(processId, ApplyMandatoryInclusion);

        return await completeQuery.AsSplitQuery().FirstAsync(ct);

        IIncludableQueryable<Process, CourseCountry> ApplyMandatoryInclusion(IQueryable<Process> query) =>
            query.Include(p => p.SelectedLeads)
                .ThenInclude(cl => cl.CourseCountry);
    }

    public async Task<Process> GetProcessesWithRelatedData(
        int processId,
        ICollection<int> onlyContactLeadIds,
        CancellationToken ct)
    {
        var completeQuery = GetProcessQueryCore(processId, ApplyMandatoryInclusion);

        return await completeQuery.AsSplitQuery().FirstAsync(ct);

        IIncludableQueryable<Process, CourseCountry> ApplyMandatoryInclusion(IQueryable<Process> query) =>
            query.Include(
                    p => p.SelectedLeads
                        .Where(cl => onlyContactLeadIds.Contains(cl.Id))
                        .Where(cl => !cl.IsDeleted))
                .ThenInclude(cl => cl.CourseCountry);
    }

    /// <summary>
    ///     Applies all the expected filtration and mandatory inclusions for object graph, that are required by this
    ///     same service later on.
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="contactLeadInclusionDlgt">
    ///     This is mandatory too, but allows to apply filtering on top of navigation property to select desired
    ///     related entities only. <br /> If filtering is not desired, then at minimum <c>Include()</c> call against
    ///     <see cref="Process.SelectedLeads" /> must be present.
    /// </param>
    /// <returns>
    ///     Complete query, with related data hinting, but ChangeTracker control is not applied, it's up to the caller.
    ///     <br />
    ///     Also, <c>SplitQuery()</c> decision it up to the caller.
    /// </returns>
    public IQueryable<Process> GetProcessQueryCore(
        int processId,
        Func<IQueryable<Process>, IIncludableQueryable<Process, CourseCountry>> contactLeadInclusionDlgt)
    {
        // TODO: Analyze the need to use `IsDeleted` in related data inclusion too!
        var baseInclusionsQuery = _dbContext.Processes
            .Include(p => p.Contact.ContactLanguages.OrderByDescending(cl => cl.IsDefault).Take(1))
            .Include(p => p.Contact.ContactEmail.OrderByDescending(cl => cl.IsDefault).Take(1))
            .Include(p => p.User.Employee)
            .Where(cl => !cl.IsDeleted);

        var completeInclusionsQuery = contactLeadInclusionDlgt(baseInclusionsQuery);

        return completeInclusionsQuery
            .Where(p => p.Id == processId)
            .Where(p => !p.IsDeleted);
    }

    public async Task<List<Process>> GetProcessesWithRelatedData(IEnumerable<int> processIds, CancellationToken ct) =>
        await _dbContext.Processes
            .Where(p => processIds.Contains(p.Id) && !p.IsDeleted)
            .Include(p => p.Contact.ContactLanguages)
            .Include(p => p.Contact.ContactEmail)
            .Include(p => p.User.Employee)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(ct);

    #endregion

    #region GetPricesForTLMK ManagementApi

    public async Task<ErrorOr<List<(ContactLead Lead, CourseImportedTlmkDto? Price)>>> GetCoursePricesFromManagementApi(
        Process process,
        CancellationToken ct)
    {
        var apiCallTasks = process.SelectedLeads
            .Select(cl => GetPricesTlmkFromManagementApiData(cl, process.Type, ct));

        var apiResults = await Task.WhenAll(apiCallTasks);

        if (apiResults.Any(r => r is null))
        {
            return Error.Failure(description: LogErrMissingCoursesPrices);
        }

        return process.SelectedLeads
            .Zip(apiResults)
            .ToList();
    }

    public Task<CourseImportedTlmkDto?> GetPricesTlmkFromManagementApiData(
        ContactLead lead,
        ProcessType processType,
        CancellationToken ct)
    {
        if (lead is not { CourseCountry: { }, CourseCode.Length: > 0 })
        {
            return Task.FromResult<CourseImportedTlmkDto?>(null);
        }

        var paramsDto = new PricesForTlmkManagementApiDto(
            lead.CourseCode,
            lead.CourseCountry.Id,
            lead.CourseTypeBaseCode,
            processType.ToString());

        return _managementApiClient.GetTlmkCoursePricesManagementApiData(paramsDto, ct);
    }

    #endregion

    #region Obtained Templates

    public async Task<Template?> GetTemplateByProcess(
        string label,
        ProcessType processType,
        int languageId,
        CancellationToken ct)
    {
        var cacheKey = new TemplateCacheKey(label, languageId);

        if (_templateCache.TryGetValue(cacheKey, out var template))
        {
            return template;
        }

        var result = await _dbContext.TemplateProposals
            .Where(tp => !tp.IsDeleted && tp.ProcessType == processType)
            .Join(
                _dbContext.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted),
                tp => tp.Id,
                tpt => tpt.TemplateProposalId,
                (tp, tpt) => new { tp, tpt })
            .Join(
                _dbContext.Templates.Where(t => !t.IsDeleted),
                tp_tpt => tp_tpt.tpt.TemplateId,
                t => t.Id,
                (tp_tpt, t) => t)
            .Include(t => t.Language)
            .Where(
                t => t.Label == label
                     && t.Type == TemplateType.Email
                     && t.TagId == TagIdCrmInternational
                     && t.LanguageId == languageId)
            .Distinct()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return _templateCache.GetOrAdd(cacheKey, result);
    }

    public async Task<Template?> GetTemplateAsync(string templateCode, int languageId, CancellationToken ct)
    {
        var cacheKey = new TemplateCacheKey(templateCode, languageId);

        if (_templateCache.TryGetValue(cacheKey, out var template))
        {
            return template;
        }

        var result = await LoadTemplate(templateCode, languageId, ct);
        _templateCache.TryAdd(cacheKey, result);

        return result;
    }

    private async Task<Template?> LoadTemplate(string templateCode, int languageId, CancellationToken ct) =>
        await _dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Language)
            .Where(
                t => !t.IsDeleted
                     && t.TagId == TagIdCrmInternational
                     && t.Name.Contains(templateCode)
                     && t.Type == TemplateType.Email
                     && t.LanguageId == languageId)
            .OrderByDescending(t => t.LanguageId == languageId)
            .FirstOrDefaultAsync(ct);

    #endregion

    #region Store Business Email Data and Overrides

    #region A single process/email

    public async Task<Action?> StoreBusinessEmailData(
        Process process,
        CreateEmail emailCommand,
        CancellationToken ct)
    {
        var dictionary = new Dictionary<Process, CreateEmail>
        {
            [process] = emailCommand,
        };

        var result = await StoreBusinessEmailData(dictionary, EntityCreatorTag, NoOpConfigureEntitiesDlgt, ct);
        return result.FirstOrDefault();
    }

    public async Task<Action?> StoreBusinessEmailData(
        Process process,
        CreateEmail emailCommand,
        string entityCreatorTag,
        CancellationToken ct)
    {
        var dictionary = new Dictionary<Process, CreateEmail>
        {
            [process] = emailCommand,
        };

        var result = await StoreBusinessEmailData(dictionary, entityCreatorTag, NoOpConfigureEntitiesDlgt, ct);
        return result.FirstOrDefault();
    }

    #endregion

    #region Many processes/emails

    public Task<List<Action>> StoreBusinessEmailData(
        Dictionary<Process, CreateEmail> processEmailDictionary,
        CancellationToken ct) =>
        StoreBusinessEmailData(processEmailDictionary, EntityCreatorTag, NoOpConfigureEntitiesDlgt, ct);

    public Task<List<Action>> StoreBusinessEmailData(Dictionary<Process, CreateEmail> processEmailDictionary,
        string entityCreatorTag,
        CancellationToken ct) =>
        StoreBusinessEmailData(processEmailDictionary, entityCreatorTag, NoOpConfigureEntitiesDlgt, ct);

    #endregion

    #region Store Business Logic Core

    public async Task<List<Action>> StoreBusinessEmailData(
        Dictionary<Process, CreateEmail> processEmailCommands,
        string entityCreatorTag,
        Action<Email, Action> configureEntitiesDlgt,
        CancellationToken ct)
    {
        //TODO:Poner un if dentro de Store y cambiar el email de pending a failure si llega a fallar.

        var allActions = new List<Action>();

        var processEmailCommandsValidated = ProcessEmailCommandsValidated(processEmailCommands);

        if (string.IsNullOrWhiteSpace(entityCreatorTag))
        {
            entityCreatorTag = EntityCreatorTag;
        }

        foreach (var chunk in processEmailCommandsValidated.Chunk(100))
        {
            foreach (var (process, emailCommand) in chunk)
            {
                using var scope = _logger.BeginScope("Processing email for ProcessId: {ProcessId}", process.Id);

                Action actions;
                try
                {
                    actions = StoreBusinessEmailCore(process, emailCommand, configureEntitiesDlgt);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        ex,
                        "Failed to storage email and action command to {Email} of {ProcessId}",
                        emailCommand,
                        process.Id);

                    continue;
                }

                allActions.Add(actions);
            }

            try
            {
                await _dbContext.SaveChangesAsync(entityCreatorTag, ct);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to save changes for {EntityCreatorTag}", entityCreatorTag);
            }
        }

        return allActions;
    }

    private Dictionary<Process, CreateEmail> ProcessEmailCommandsValidated(
        Dictionary<Process, CreateEmail> processEmailCommands)
    {
        if (processEmailCommands.All(IsValid))
        {
            return processEmailCommands;
        }

        var validatedProcessEmailCommands = processEmailCommands.Where(IsValid)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        HandleInvalidProcessEmailCommands(processEmailCommands, validatedProcessEmailCommands);

        return validatedProcessEmailCommands;

        bool IsValid(KeyValuePair<Process, CreateEmail> x) => x.Value.CorrelationId != Guid.Empty;
    }

    private void HandleInvalidProcessEmailCommands(
        Dictionary<Process, CreateEmail> processEmailCommands,
        Dictionary<Process, CreateEmail> validatedProcessEmailCommands)
    {
        var validatedProcesses = validatedProcessEmailCommands.Select(kvp => kvp.Key);

        var invalidProcessEmailCommands = processEmailCommands.ExceptBy(validatedProcesses, kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var failedProcessIds = invalidProcessEmailCommands.Select(x => x.Key.Id);

        var failedProcessIdsSb = new StringBuilder(invalidProcessEmailCommands.Count)
            .AppendJoin(';', failedProcessIds);

        _logger.LogError(
            "Processes with missing Email CorrelationIds {FailedProcessIds}",
            failedProcessIdsSb.ToString());
    }

    private Action StoreBusinessEmailCore(
        Process process,
        CreateEmail emailCommand,
        Action<Email, Action> configureEntitiesDlgt)
    {
        var action = AddAction(process);
        var email = AddEmail(process, emailCommand);

        action.Guid = emailCommand.CorrelationId;
        action.Email = email;

        configureEntitiesDlgt(email, action);

        return action;
    }

    #endregion

    #endregion

    public int GetContactLanguageIdOrDefault(Process process)
    {
        var contactLanguage = process.Contact.ContactLanguages
            .Where(cl => !cl.IsDeleted)
            .MaxBy(cl => cl.IsDefault);

        if (contactLanguage?.LanguageId is { } langId)
        {
            return langId;
        }

        _logger.LogWarning(
            "Cannot get Contact Language Id from {ProcessId}, using {DefaultLanguageIdEs}",
            process.Id,
            DefaultLanguageIdEs);

        return DefaultLanguageIdEs;
    }

    /// <summary>
    ///     Assigns almost all the properties, but most notable (relational) property is not assigned is
    ///     <see cref="Email.ContactLeadId" />.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="rawEmail"></param>
    /// <returns></returns>
    private Email AddEmail(Process process, CreateEmail rawEmail)
    {
        var to = SerializeItems(rawEmail.Receivers);
        var cc = SerializeItems(rawEmail.CcReceivers);
        var bcc = SerializeItems(rawEmail.BccReceivers);

        var email = new Email
        {
            FromName = rawEmail.FromName,
            From = rawEmail.From,
            To = to,
            Cc = cc,
            Bcc = bcc,
            Body = rawEmail.Body,
            Subject = rawEmail.Subject,
            ContactId = process.ContactId,
            UserId = process.UserId,
            ProcessId = process.Id,
        };

        return _dbContext.Emails.Add(email).Entity;
    }

    private Action AddAction(Process process)
    {
        var finishDate = DateTime.UtcNow;

            var action = new Action
            {
                UserId = process.UserId,
                ContactId = process.ContactId,
                Date = finishDate,
                FinishDate = finishDate,
                Type = ActionType.EmailPending,
                Outcome = ActionOutcome.Sale,
                ProcessId = process.Id,
            };

        return _dbContext.Actions.Add(action).Entity;
    }

    #region Email Signature

    public async Task<(string subject, string body)> AddSignatureToEmailAsync(
        string subject,
        string body,
        CancellationToken ct)
    {
        var (hashtag, cleanSubject) = ExtractHashtag(subject);

        if (hashtag is null)
        {
            return (subject, body);
        }

        var blobContent = await GetBlobSignatureAsync(hashtag, ct);
        return (cleanSubject, AppendSignatureToBody(body, blobContent));
    }

    private (string? Hashtag, string CleanSubject) ExtractHashtag(string subject)
    {
        var words = subject.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hashtag = words.FirstOrDefault(word => word.StartsWith('#'));

        if (hashtag is null)
        {
            return (null, subject);
        }

        var cleanSubject = subject.Replace(hashtag, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return (hashtag, cleanSubject);
    }

    private async Task<string?> GetBlobSignatureAsync(string hashtag, CancellationToken ct)
    {
        var blobName = $"{hashtag}.html";
        return await _blobStorageService.GetBlobContentAsync(
            blobName,
            _connectionStringSignature,
            _containerSignatureName,
            ct);
    }

    private string AppendSignatureToBody(string body, string? blobContent) =>
        !string.IsNullOrEmpty(blobContent)
            ? $"{body}<br>{blobContent}"
            : body;

    #endregion

    public Task SendCreateEmailCommand(CreateEmail emailCommand, CancellationToken ct) => _bus.Send(emailCommand, ct);

    public async Task<List<int>> SendAllEmails(
        Dictionary<Process, CreateEmail> processEmailCommands,
        CancellationToken ct)
    {
        var tasks = new List<(int PId, Task Task)>(processEmailCommands.Count);

        foreach (var (process, createEmail) in processEmailCommands)
        {
            try
            {
                var task = _bus.Send(createEmail, ct);
                tasks.Add((process.Id, task));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to send email sending command to {ReceiverEmail} of {ProcessId}",
                    SerializeItems(createEmail.Receivers),
                    process.Id);
            }
        }

        await Task.WhenAll(tasks.Select(x => x.Task));

        return tasks.Select(t => t.PId)
            .ToList();
    }

    #region Utils

    public Func<string?>? ConvocationDateDelegate(bool useConvocationDate)
    {
        Func<string?>? convocationDateDelegate = null;
        if (useConvocationDate)
        {
            var convocationDate = ConvocationDateAlgorithms.GetNextOfTheNext();
            convocationDateDelegate = () => convocationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return convocationDateDelegate;
    }


    public decimal SanitizeDiscountPercentage(decimal? rawDiscount)
    {
        if (rawDiscount is { } d and not 100)
        {
            return d;
        }

        _logger.LogWarning(
            "Discount sources didn't yield value; using {ScholarshipDiscountPercentFallback}",
            _scholarshipDiscountPercentFallback);

        return _scholarshipDiscountPercentFallback;
    }

    public string CreateResultMessage(int countOfEmailsSent, List<Process> processes) =>
        $"OK. {countOfEmailsSent} of {processes.Count} emails sent.";

    /// <summary>
    ///     For service's private use only.
    /// </summary>
    /// <param name="Code"></param>
    /// <param name="LanguageId"></param>
    private readonly record struct TemplateCacheKey(string Code, int LanguageId);

    /// <summary>
    ///     Joins elements into coma-separated string.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    private static string SerializeItems(List<string>? items) =>
        items is [_, ..]
            ? string.Join(',', items)
            : string.Empty;

    #endregion
}
