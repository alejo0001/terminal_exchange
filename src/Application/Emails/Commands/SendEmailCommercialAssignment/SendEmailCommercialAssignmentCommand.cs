using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;
using static CrmAPI.Application.Common.Utils.EmailContentHelper;
using static CrmAPI.Application.TemplateLabelRegistry;

namespace CrmAPI.Application.Emails.Commands.SendEmailCommercialAssignment;

public record SendEmailCommercialAssignmentCommand : Contracts.Commands.SendEmailCommercialAssignment, IRequest<string>
{
    public required string ApiKey { get; init; }
}

[UsedImplicitly]
public class SendEmailCommercialAssignmentCommandHandler : IRequestHandler<SendEmailCommercialAssignmentCommand, string>
{
    private readonly ICommercialAssignmentService _commercialAssignmentService;
    private readonly ICrmMailerService _crmMailerService;
    private readonly ILogger<SendEmailCommercialAssignmentCommandHandler> _logger;

    public SendEmailCommercialAssignmentCommandHandler(
        ICommercialAssignmentService commercialAssignmentService,
        IConfiguration configuration,
        ILogger<SendEmailCommercialAssignmentCommandHandler> logger, 
        ICrmMailerService crmMailerService)
    {
        _commercialAssignmentService = commercialAssignmentService;
        _logger = logger;
        _crmMailerService = crmMailerService;
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
    public async Task<string> Handle(SendEmailCommercialAssignmentCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Started execution");

        if (await _crmMailerService.GetProcessesWithRelatedData(request.ProcessIds, ct) is not [_, ..] processes)
        {
            _logger.LogCritical("Null or Empty processList, use case execution halted");

            return "Error with processList in SendEmailV2";
        }
        
        var processEmailCommands = await CreateEmailCommands(processes, ct);
        
        if (processEmailCommands.Count == 0)
        {
            _logger.LogWarning("No email commands provided to process.");
            return "Process or Email error. See Logs for more information.";
        }

        await _crmMailerService.StoreBusinessEmailData(processEmailCommands, ct);

        var succeedProcessIds = await _crmMailerService.SendAllEmails(processEmailCommands, ct);

        var resultMessage = _crmMailerService.CreateResultMessage(succeedProcessIds.Count, processes);

        _logger.LogInformation("Finished execution with `{Result}`", resultMessage);

        return resultMessage;
    }

    private async Task<Dictionary<Process, CreateEmail>> CreateEmailCommands(List<Process> processes, CancellationToken ct)
    {
        var result = new Dictionary<Process, CreateEmail>(processes.Count);

        await _commercialAssignmentService.FillOrganizationNodeCache(ct);

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
                
                using var _ = _logger.BeginScope("{ProcessId}", process.Id);

                var languageId = _crmMailerService.GetContactLanguageIdOrDefault(process);

                if (await _crmMailerService.GetTemplateByProcess(CommercialAssignment, process.Type, languageId, ct)
                    is not { } template)
                {
                    _logger.LogError(
                        "Cannot get Template using {Label}, {ProcessType} and {LanguageId}, skipping",
                        CommercialAssignment,
                        process.Type,
                        languageId);

                    continue;
                }

                var employee = process.User.Employee;
                //forma correcta
                //var manager = IEmployeeService.GetManagerByEmployee(process.User.Employee, ct);
                // ñapa
                if (_commercialAssignmentService.GetEmployeeManager(employee) is not { } manager)
                {
                    _logger.LogError("Cannot get Employees {EmployeeEmail} Manager, skipping", employee.CorporateEmail);

                    continue;
                }

                var body = ReplaceRecords2BodyTemplate(template, employee, manager, process);

                var convocationDate = _crmMailerService.ConvocationDateDelegate();

                var subject = GenerateSubjectFromTemplate(template.Subject, template.LanguageId, convocationDate);
                
                var emailContract = await _crmMailerService.BuildEmail(process, receiver, template.FromEmail, subject, body, ct);

                result.Add(process, emailContract);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to create email sending command to {ReceiverEmail} of {ProcessId}",
                    EmailHeadersHelper.GetReceiver(process),
                    process.Id);
            }
        }

        return result;
    }
}
