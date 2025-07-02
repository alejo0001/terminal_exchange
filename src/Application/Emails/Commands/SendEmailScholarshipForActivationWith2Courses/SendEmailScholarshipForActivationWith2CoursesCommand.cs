using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.BusinessAlgorithms;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using CrmAPI.Contracts.Commands;
using CrmAPI.Contracts.Dtos;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Emails.Commands.SendEmailScholarshipForActivationWith2Courses;

/// <inheritdoc cref="ISendEmailScholarshipForActivationWith2Courses" />
/// <remarks>
///     As this command is specific to Template and business use case, only first 2
///     <see cref="SendEmailScholarshipForActivationWith2CoursesDto.ContactLeadIds" /> will be processed.<br />
///     NB! No security, only used in messaging communication; add ApiKey ASAP in case of exposing it via WebApi.
/// </remarks>
public sealed record SendEmailScholarshipForActivationWith2CoursesCommand(
    SendEmailScholarshipForActivationWith2CoursesDto Dto)
    : IRequest<ErrorOr<Success>>;

[UsedImplicitly]
public sealed partial class SendEmailScholarshipForActivationWith2CoursesCommandHandler
    : IRequestHandler<SendEmailScholarshipForActivationWith2CoursesCommand, ErrorOr<Success>>
{
    private const string EntityCreatorTag = nameof(SendEmailScholarshipForActivationWith2CoursesCommandHandler);

    private const string LogErrOnStoringCustomerEmailAndCommunicationMetadata
        = "Error on storing customer email and communication metadata";

    private const string LogErrFailedToSendEmailCommand
        = "Failed to send email command; set Action Type to failure; job halted";

    private const string LogErrMissingCoursesPrices = "Missing one or more courses' prices; job halted";

    private readonly IApplicationDbContext _context;
    private readonly ICrmMailerService _crmMailerService;
    private readonly IEmailSendTestingFeatureFlagService _emailSendTestingFeatureFlagService;
    private readonly ILogger<SendEmailScholarshipForActivationWith2CoursesCommandHandler> _logger;

    private readonly string _auditEmailAddress;
    private readonly string _fundeumDefaultFromAddress;
    private readonly decimal _scholarshipDiscountPercentFallback;

    public SendEmailScholarshipForActivationWith2CoursesCommandHandler(
        IApplicationDbContext context,
        ICrmMailerService crmMailerService,
        IEmailSendTestingFeatureFlagService  emailSendTestingFeatureFlagService,
        IConfiguration configuration,
        ILogger<SendEmailScholarshipForActivationWith2CoursesCommandHandler> logger)
    {
        _context = context;
        _crmMailerService = crmMailerService;
        _emailSendTestingFeatureFlagService = emailSendTestingFeatureFlagService;
        _logger = logger;

        _auditEmailAddress = configuration["Constants:AuditEmailAddress"]!;
        _fundeumDefaultFromAddress = configuration["Constants:FundeumEmailAddress"]!;
        _scholarshipDiscountPercentFallback = configuration.GetValue<decimal>(
            "Constants:ScholarshipDiscountPercentFallback");
    }

    public async Task<ErrorOr<Success>> Handle(
        SendEmailScholarshipForActivationWith2CoursesCommand request,
        CancellationToken ct)
    {
        using var _1 = _logger.BeginScope("{@Dto}", request.Dto);
        _logger.LogTrace("Starting; {TemplateCode}", TemplateCodeRegistry.ScholarshipForActivationWith2Courses);

        request.Dto.Deconstruct(out _, out var processId, out var contactLeadIds);

        var process = await _crmMailerService.GetProcessesWithRelatedData(processId, contactLeadIds, ct);

        var receiver = EmailHeadersHelper.GetReceiver(process);

        if (string.IsNullOrWhiteSpace(receiver))
        {
            _logger.LogError("Receiver is Null or Empty, its impossible to resolve.");
            return Error.NotFound();
        }

        var coursePricesResult = await GetCoursePrices(process, ct);
        if (coursePricesResult.IsError)
        {
            _logger.LogError(LogErrMissingCoursesPrices);

            return coursePricesResult.Errors;
        }

        var (languageId, isFallbackLanguageId) = LanguageExtractionAlgorithms.GetContactLanguageIdOrDefault(process);

        if (isFallbackLanguageId)
        {
            LogWarningFallbackToDefaultLanguage(_logger, process.Id, languageId);
        }

        if (await _crmMailerService.GetTemplateAsync(
                TemplateCodeRegistry.ScholarshipForActivationWith2Courses,
                languageId,
                ct)
            is not { } template)
        {
            LogErrorNoTemplate(_logger, TemplateCodeRegistry.ScholarshipForActivationWith2Courses, languageId);

            return Error.NotFound();
        }

        var emailCommand = GetEmailCommand(template, process, receiver, coursePricesResult.Value);

        using var _2 = _logger.BeginScope("{@EmailCommand}", emailCommand);

        Action? actionEntity;
        try
        {
            actionEntity = await _crmMailerService.StoreBusinessEmailData(process, emailCommand, EntityCreatorTag, ct);
        }
        catch (Exception ex)
        {
            return HandleDataStoringFailures(ex);
        }

        if (actionEntity is null)
        {
            _logger.LogCritical("Action is Null. Problems storing business Email Data.");
            return Error.NotFound();
        }

        using var _3 = _logger.BeginScope("{ActionId}", actionEntity.Id);
        try
        {
            await _crmMailerService.SendCreateEmailCommand(emailCommand, ct);
        }
        catch (Exception ex)
        {
            return await HandleEmailSendingFailure(ex, actionEntity, ct);
        }

        _logger.LogTrace("End; sent and email {@To} of {ActionId}", receiver, actionEntity.Id);

        return Result.Success;
    }

    private async Task<ErrorOr<List<(ContactLead Lead, CourseImportedTlmkDto? Price)>>> GetCoursePrices(
        Process process,
        CancellationToken ct)
    {
        var apiCallTasks = process.SelectedLeads
            .Select(cl => _crmMailerService.GetPricesTlmkFromManagementApiData(cl, process.Type, ct));

        var apiResults = await Task.WhenAll(apiCallTasks);

        if (apiResults.Any(r => r is null))
        {
            return Error.Failure(description: LogErrMissingCoursesPrices);
        }

        return process.SelectedLeads
            .Zip(apiResults)
            .ToList();
    }

    private CreateEmail GetEmailCommand(
        Template template,
        Process process,
        string receiver,
        List<(ContactLead Lead, CourseImportedTlmkDto? Price)> courseImportedTlmkDtos)
    {
        var convocationDate = ConvocationDateAlgorithms.GetNextOfTheNext();

        var subject = EmailContentHelper.GenerateSubjectFromTemplate(template.Subject, template.LanguageId, ConvocationDateDlgt);
        var from = EmailHeadersHelper.GetFrom(process, template.FromEmail, _fundeumDefaultFromAddress);
        var fromName = EmailHeadersHelper.GetFromName(process);
        var receivers = new List<string> { receiver };

        var body = GetBody(
            template,
            process,
            courseImportedTlmkDtos,
            ConvocationDateDlgt,
            SanitizeDiscountPercentage);

        var isTestEmail = _emailSendTestingFeatureFlagService.IsAutomaticEmailSendingForcedToTestOnly;

        var emailCommand = new CreateEmail
        {
            CorrelationId = NewId.NextSequentialGuid(),
            Subject = subject,
            From = from,
            Body = body,
            FromName = fromName,
            Receivers = receivers,
            BccReceivers = new() { _auditEmailAddress },
            UseExchange = true, // TODO: Put into config & add signature generation condition.
            IsTestEmail = isTestEmail,
        };

        return emailCommand;

        string ConvocationDateDlgt() => convocationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private decimal SanitizeDiscountPercentage(decimal? rawDiscount)
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

    /// <summary>
    ///     TODO: I'mm not happy with it, because it performs scan-replace 2x!
    /// </summary>
    /// <param name="template"></param>
    /// <param name="process"></param>
    /// <param name="coursePrices"></param>
    /// <param name="convocationDateDlgt"></param>
    /// <param name="sanitizeDiscountPercentDlgt">
    ///     See <see cref="ProductPricingAlgorithms.GetScholarshipDiscountPercent" /> for more details.
    /// </param>
    /// <returns></returns>
    private static string GetBody(
        Template template,
        Process process,
        List<(ContactLead Lead, CourseImportedTlmkDto? Price)> coursePrices,
        Func<string> convocationDateDlgt,
        Func<decimal?, decimal> sanitizeDiscountPercentDlgt)
    {
        var convocationDate = convocationDateDlgt();

        var coursePrices1 = coursePrices[0];
        var coursePrices2 = coursePrices[1];

        Func<ContactLead?, CourseImportedTlmkDto?, ProductPricingAlgorithms.DiscountPercent> discountPercentDlgt =
            (cl, tlmkDto) =>
                ProductPricingAlgorithms.GetScholarshipDiscountPercent(cl, tlmkDto, sanitizeDiscountPercentDlgt);

        var body = EmailContentHelper.ReplaceGeneralScholarshipActivation2BodyTemplate(
            template.Body,
            coursePrices1.Lead,
            coursePrices1.Price,
            process,
            convocationDate,
            discountPercentDlgt);

        body = EmailContentHelper.ReplaceScholarshipActivationSecondCourseTemplate(
            body,
            coursePrices2.Lead,
            coursePrices2.Price,
            process,
            convocationDate,
            discountPercentDlgt);

        return body;
    }

    private Error HandleDataStoringFailures(Exception ex)
    {
        _logger.LogCritical(
            ex,
            "Failed to send email command; {Reason}; job halted",
            LogErrOnStoringCustomerEmailAndCommunicationMetadata);

        return Error.Unexpected(description: LogErrOnStoringCustomerEmailAndCommunicationMetadata);
    }

    /// <summary>
    ///     Log exception as Critical error message. Set created <see cref="Action" />`s Type to
    ///     <see cref="ActionType.EmailFailed" />.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="actionEntity"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<Error> HandleEmailSendingFailure(
        Exception ex,
        Action actionEntity,
        CancellationToken ct)
    {
        _logger.LogCritical(ex, LogErrFailedToSendEmailCommand);

        actionEntity.Type = ActionType.EmailFailed;
        await _context.SaveChangesAsync(EntityCreatorTag, ct);

        return Error.Unexpected(description: LogErrFailedToSendEmailCommand);
    }

    #region Logging

    [LoggerMessage(1, LogLevel.Warning, "Cannot get Contact Language Id from {ProcessId}, using {DefaultLanguageId}")]
    private static partial void LogWarningFallbackToDefaultLanguage(ILogger l, int processId, int defaultLanguageId);

    [LoggerMessage(6, LogLevel.Error, "Cannot get Template using {TemplateCode} and {LanguageId}, terminate")]
    private static partial void LogErrorNoTemplate(ILogger l, string templateCode, int languageId);

    #endregion
}
