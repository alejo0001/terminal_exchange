using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.BusinessAlgorithms;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Entities;
using MediatR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;
using static CrmAPI.Application.Common.Utils.EmailContentHelper;
using static CrmAPI.Application.TemplateCodeRegistry;

namespace CrmAPI.Application.Emails.Commands.SendEmailRecords2ScholarshipActivation;

public record SendEmailRecords2ScholarshipActivationCommand
    : Contracts.Commands.SendEmailRecords2ScholarshipActivation, IRequest<string>
{
    public required string ApiKey { get; init; }
}

[UsedImplicitly]
public class SendEmailRecords2ScholarshipActivationCommandHandler
    : IRequestHandler<SendEmailRecords2ScholarshipActivationCommand, string>
{

    private readonly IApplicationDbContext _context;
    private readonly ICrmMailerService _crmMailerService;
    private readonly ILogger<SendEmailRecords2ScholarshipActivationCommandHandler> _logger;

    public SendEmailRecords2ScholarshipActivationCommandHandler(
        ILogger<SendEmailRecords2ScholarshipActivationCommandHandler> logger, 
        ICrmMailerService crmMailerService, 
        IApplicationDbContext context)
    {
        _logger = logger;
        _crmMailerService = crmMailerService;
        _context = context;
    }

    /// <summary>
    ///     Sends emails using a specified template for each process in the provided list.
    ///     The function retrieves the employee based on the current user's email and processes each item in the list.
    ///     For each process, it identifies the preferred contact language, fetches the corresponding contact lead,
    ///     and retrieves the appropriate email template.
    ///     The function then constructs the email content, including subject, body, and recipient information,
    ///     before publishing the email contract to the messaging bus.
    /// </summary>
    /// <returns>
    ///     Returns a descriptive string both on error and success scenarios.
    ///     TODO: use Result Pattern to give meaningfully error info.
    /// </returns>
    public async Task<string> Handle(SendEmailRecords2ScholarshipActivationCommand request, CancellationToken ct)
    {
        return await EmailRecords2ScholarshipActivationStepByStep(request.ProcessIds, ct);
    }

    private async Task<string> EmailRecords2ScholarshipActivationStepByStep(List<int> processList,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Started execution of {EmailRecords2ScholarshipActivationStepByStep}",
            nameof(EmailRecords2ScholarshipActivationStepByStep));

        if (await _crmMailerService.GetProcessesWithRelatedData(processList, ct) is not [_, ..] processes)
        {
            _logger.LogCritical("Null or Empty processList, use case execution halted");

            return "Error with processList in SendEmailV2";
        }

        var processEmailCommands = await CreateEmailForRecords2ScholarshipActivation(processes, ct);
        
        using var _2 = _logger.BeginScope("{@EmailCommand}", processEmailCommands);
        
        if (processEmailCommands.Count == 0)
        {
            _logger.LogWarning("No email commands provided to process.");
            return "Process or Email error. See Logs for more information.";
        }

        await _crmMailerService.StoreBusinessEmailData(processEmailCommands, ct);

        var succeedProcessIds = await _crmMailerService.SendAllEmails(processEmailCommands, ct);

        var resultMessage = _crmMailerService.CreateResultMessage(succeedProcessIds.Count, processes);

        _logger.LogInformation(
            "Finished execution of {EmailRecords2ScholarshipActivationStepByStep}, {Result}",
            nameof(EmailRecords2ScholarshipActivationStepByStep),
            resultMessage);

        return resultMessage;
    }

    private async Task<Dictionary<Process, CreateEmail>> CreateEmailForRecords2ScholarshipActivation(
        List<Process> processes, CancellationToken ct)
    {
        var result = new Dictionary<Process, CreateEmail>(processes.Count);

        foreach (var process in processes)
        {
            try
            {
                var receiver = EmailHeadersHelper.GetReceiver(process);
        
                if (string.IsNullOrWhiteSpace(receiver))
                {
                    _logger.LogError("Receiver is Null in ProcessId: {ProcessId} and ContactId: {ContactId}", 
                        process.Id, process.ContactId);
                    continue;
                }
                
                var contactLanguageId = _crmMailerService.GetContactLanguageIdOrDefault(process);

                var contactLeads = await GetContactLeads(process.ContactId, 2, ct);
                if (contactLeads is not [_, ..])
                {
                    _logger.LogWarning(
                        "Cannot get ContactLead for {ContactId}, switching {IntendedTemplateCode} to {FallbackTemplateCode} "
                        + ", continuing",
                        process.ContactId,
                        ScholarshipActivationR2,
                        NoInterestedCourseR2);

                    contactLeads.Add(new());
                }

                var templateCode = GetTemplateCodeVariable(contactLeads);
                
                var coursesImported = await GetDataCoursesImportedFromManagementApi(process, contactLeads, ct);
                if (coursesImported is not [_, ..])
                {
                    _logger.LogWarning(
                        "Cannot get Course and its price data from TLMK, switching {IntendedTemplateCode} to "
                        + "{FallbackTemplateCode}, continuing",
                        ScholarshipActivationR2,
                        NoInterestedCourseR2);

                    coursesImported.Add(new());

                    templateCode = NoInterestedCourseR2;
                }
                
                if (await _crmMailerService.GetTemplateAsync(templateCode, contactLanguageId, ct)
                    is not { } template)
                {
                    _logger.LogError(
                        "Cannot get Template using {TemplateCode} and {ContactLanguageId}, skipping",
                        templateCode,
                        contactLanguageId);

                    continue;
                }

                var convocationDateDelegate = _crmMailerService.ConvocationDateDelegate();
                var convocationDate = convocationDateDelegate();

                var body = GetBody(
                    template.Body,
                    contactLeads,
                    coursesImported,
                    process,
                    convocationDate,
                    templateCode,
                    _crmMailerService.SanitizeDiscountPercentage);

                var subject = GenerateSubjectFromTemplate(template.Subject, template.LanguageId, convocationDateDelegate);

                var emailContract = await _crmMailerService.BuildEmail(process, receiver, template.FromEmail, subject, body, ct);

                result.Add(process, emailContract);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to create email command to {ReceiverEmail} of {ProcessId}", 
                    EmailHeadersHelper.GetReceiver(process),
                    process.Id);
            }
        }
        return result;
    }

    private static string GetBody(
        string body,
        List<ContactLead> contactLeads,
        List<CourseImportedTlmkDto> coursesImported,
        Process process,
        string convocationDateDlgt,
        string templateCode,
        Func<decimal?, decimal> sanitizeDiscountPercentDlgt)
    {
        Func<ContactLead?, CourseImportedTlmkDto?, ProductPricingAlgorithms.DiscountPercent> discountPercentDlgt =
            (cl, tlmkDto) =>
                ProductPricingAlgorithms.GetScholarshipDiscountPercent(cl, tlmkDto, sanitizeDiscountPercentDlgt);

        var newBody = ReplaceGeneralScholarshipActivation2BodyTemplate(
            body,
            contactLeads[0],
            coursesImported[0],
            process,
            convocationDateDlgt,
            discountPercentDlgt);

        if (templateCode == ScholarshipActivationR2Courses)
        {
            newBody = ReplaceScholarshipActivationSecondCourseTemplate(
                newBody,
                contactLeads[1],
                coursesImported[1],
                process,
                convocationDateDlgt,
                discountPercentDlgt);
        }

        return newBody;
    }

    private async Task<List<CourseImportedTlmkDto>> GetDataCoursesImportedFromManagementApi(
        Process process,
        List<ContactLead> contactLeads,
        CancellationToken ct)
    {
        var coursesImportedFromTlmk = new List<CourseImportedTlmkDto>();

        foreach (var contactLead in contactLeads)
        {
            var courseImportedTlmk = await _crmMailerService.GetPricesTlmkFromManagementApiData(contactLead, process.Type, ct);
            if (courseImportedTlmk.HasValue)
            {
                coursesImportedFromTlmk.Add(courseImportedTlmk.Value);
            }
        }

        return coursesImportedFromTlmk;
    }
    
    private async Task<List<ContactLead>> GetContactLeads(int contactId, int numberOfContacts, CancellationToken ct) =>
        await _context.ContactLeads
            .Where(cle => cle.ContactId == contactId 
                          && !cle.IsDeleted 
                          && cle.CourseCode != null)
            .Include(cl => cl.CourseCountry)
            .Include(cl => cl.Faculty)
            .OrderByDescending(cle => cle.LastModified)
            .Take(numberOfContacts)
            .ToListAsync(ct);
    
    private string GetTemplateCodeVariable(List<ContactLead> contactLeads)
    {
        return contactLeads.Count switch
        {
            1 => ScholarshipActivationR2,
            >= 2 => ScholarshipActivationR2Courses,
            _ => NoInterestedCourseR2,
        };
    }

}
