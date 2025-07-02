using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Entities;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;

namespace CrmAPI.Application.Emails.Services;

public class CloseProcessService : ICloseProcessService
{

    private readonly ICrmMailerService _crmMailerService;
    private readonly ILogger<CrmMailerService> _logger;

    public CloseProcessService(ICrmMailerService crmMailerService, ILogger<CrmMailerService> logger)
    {
        _crmMailerService = crmMailerService;
        _logger = logger;
    }
    
    public async Task<string> EmailCloseProcessStepByStep(List<int> processList, CancellationToken ct)
    {
        _logger.LogInformation(
            "Started execution of {EmailCloseProcessStepByStep}",
            nameof(EmailCloseProcessStepByStep));
        
        if (await _crmMailerService.GetProcessesWithRelatedData(processList, ct) is not [_, ..] processes)
        {
            _logger.LogCritical("Null or Empty processList, use case execution halted");

            return "Null or Empty processList, use case execution halted";
        }

        var processEmailRawCommands = await CreateEmailCommandsForCloseProcess(processes, ct);
        
        using var _2 = _logger.BeginScope("{@EmailCommand}", processEmailRawCommands);
        
        if (processEmailRawCommands.Count == 0)
        {
            _logger.LogWarning("No email commands provided to process.");
            return "Process or Email error. See Logs for more information.";
        }

        await _crmMailerService.StoreBusinessEmailData(processEmailRawCommands, ct);
        
        var succeedProcessIds = await _crmMailerService.SendAllEmails(processEmailRawCommands, ct);

        var resultMessage = _crmMailerService.CreateResultMessage(succeedProcessIds.Count, processes);

        _logger.LogInformation(
            "Finished execution of {EmailCloseProcessStepByStep}, {Result}",
            nameof(EmailCloseProcessStepByStep),
            resultMessage);

        return resultMessage;
    }
    
    public async Task<string> EmailCloseProcessStepByStep(List<Process> processes, CancellationToken ct)
    {
        _logger.LogInformation(
            "Started execution of {EmailCloseProcessStepByStep}",
            nameof(EmailCloseProcessStepByStep));
        
        var processEmailRawCommands = await CreateEmailCommandsForCloseProcess(processes, ct);
        
        using var _2 = _logger.BeginScope("{@EmailCommand}", processEmailRawCommands);
        
        if (processEmailRawCommands.Count == 0)
        {
            _logger.LogWarning("No email commands provided to process.");
            return "Process or Email error. See Logs for more information.";
        }

        await _crmMailerService.StoreBusinessEmailData(processEmailRawCommands, ct);
        
        var succeedProcessIds = await _crmMailerService.SendAllEmails(processEmailRawCommands, ct);

        var resultMessage = _crmMailerService.CreateResultMessage(succeedProcessIds.Count, processes);

        _logger.LogInformation(
            "Finished execution of {EmailCloseProcessStepByStep}, {Result}",
            nameof(EmailCloseProcessStepByStep),
            resultMessage);

        return resultMessage;
    }
    
    public async Task<Dictionary<Process, CreateEmail>> CreateEmailCommandsForCloseProcess(
        List<Process> processes,
        CancellationToken ct)
    {
        var result = new Dictionary<Process, CreateEmail>(processes.Count);

        foreach (var process in processes)
        {
            var receiver = EmailHeadersHelper.GetReceiver(process);
        
            if (string.IsNullOrWhiteSpace(receiver))
            {
                _logger.LogError("Receiver is Null in ProcessId: {ProcessId} and ContactId: {ContactId}", 
                    process.Id, process.ContactId);
                continue;
            }
            
            var languageId = _crmMailerService.GetContactLanguageIdOrDefault(process);
            
            if (await _crmMailerService.GetTemplateAsync(TemplateCodeRegistry.CloseProcesses, languageId, ct)
                is not { } template)
            {
                _logger.LogError(
                    "Cannot get Template using {TemplateCode} and {LanguageId}, skipping",
                    TemplateCodeRegistry.CloseProcesses,
                    languageId);

                continue;
            }

            var body = EmailContentHelper.ReplaceBodyTemplateGeneralVariables(template.Body, process);
            var subject = EmailContentHelper.GenerateSubjectFromTemplate(template.Subject, template.LanguageId,
                _crmMailerService.ConvocationDateDelegate(false));
            var emailContract = await _crmMailerService.BuildEmail(process, receiver, template.FromEmail, subject, body, ct);
            
            result.Add(process, emailContract);
        }

        return result;
    }
}