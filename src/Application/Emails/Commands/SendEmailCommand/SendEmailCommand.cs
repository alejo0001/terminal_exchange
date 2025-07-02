using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;
using Email = IntranetMigrator.Domain.Entities.Email;
using EmailAttachment = NotificationAPI.Contracts.Common.EmailAttachment;


namespace CrmAPI.Application.Emails.Commands.SendEmailCommand;

public class SendEmailCommand : EmailSendDto, IRequest<int>
{
    public List<IFormFile>? Attachments { get; set; }
}

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IConfiguration _configuration;
    private readonly IEmployeeService _employeeService;
    private readonly IActionsService _actionService;
    private readonly IProcessesService _processesService;
    private readonly IBus _bus;
    private readonly ICrmMailerService _crmMailerService;
    private readonly string _connectionString;
    private readonly string _containerName;

    public SendEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IBlobStorageService blobStorageService,
        IConfiguration configuration,
        IEmployeeService employeeService,
        IActionsService actionService,
        IProcessesService processesService,
        IBus bus,
        IAppSettingsService appSettingsService,
        ICrmMailerService crmMailerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _blobStorageService = blobStorageService;
        _configuration = configuration;
        _employeeService = employeeService;
        _actionService = actionService;
        _processesService = processesService;
        _bus = bus;
        var appSettingsService1 = appSettingsService;
        _connectionString = appSettingsService1["Blob:ConnectionString"];
        _containerName = appSettingsService1["Blob:ContainerName"];
        _crmMailerService = crmMailerService;
    }

    public async Task<int> Handle(SendEmailCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, ct);

        if (user == null)
        {
            throw new ForbiddenAccessException();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // TODO: cambiar ContactLeadId a un nombre que indique q es un array
            if (request.ContactLeadId is { Count: > 0 })
            {
                var leads = _context.ContactLeads.Where(lead => request.ContactLeadId.Contains(lead.Id));
                foreach (var lead in leads)
                {
                    lead.SentEmail = true;
                    _context.ContactLeads.Update(lead);
                }
            }

            var contact = await _context.Contact.FirstOrDefaultAsync(c => c.Id == request.ContactId, ct);

            var action = new Action
            {
                Guid = Guid.NewGuid(),
                ProcessId = request.ProcessId,
                ContactId = request.ContactId,
                UserId = user.Id,
                Type = ActionType.EmailPending,
                Date = _dateTime.Now,
                Outcome = ActionOutcome.CommercialEmail,
            };

            action = await _actionService.CreateAction(action, ct);

            var languageId = contact?.ContactLanguages?.FirstOrDefault()?.LanguageId ?? 1;

            var requestSubject = EmailContentHelper.FormatEmailSubjectRtl(request.Subject, languageId);
            
            var (subject, body) = await _crmMailerService.AddSignatureToEmailAsync(requestSubject, request.Body, ct);
            
            var email = new Email
            {
                ContactId = request.ContactId,
                Subject = subject,
                Cc = request.Cc,
                Bcc = request.Bcc,
                Body = body,
                FromName = $"{user.Name} {user.Surname} | Tech",
                UserId = user.Id,
                ProcessId = request.ProcessId,
                From = !string.IsNullOrEmpty(request.EmailDefault) ? request.EmailDefault : (user.Employee.CorporateEmail ?? _configuration["Constants:DefaultIntranetAddress"]),
                To = string.IsNullOrEmpty(request.To) ? contact?.Email : request.To,
                CourseId = request.CourseId,
            };
            _context.Emails.Add(email);

            var emailContract = new CreateEmail
            {
                CorrelationId = action.Guid,
                From = !string.IsNullOrEmpty(request.EmailDefault) ? request.EmailDefault : (user.Employee.CorporateEmail ?? _configuration["Constants:DefaultIntranetAddress"]!),
                Subject = subject,
                Receivers = new List<string>
                {
                    email.To!
                },
                Body = body,
                BccReceivers = new List<string>(),
                UseExchange = true
            };

            if (_configuration.GetValue<bool>("Constants:SendEmailSupervisor"))
            {
                var manager = await _employeeService.GetManagerByEmployee(user.Employee, ct);
                if (_configuration.GetValue<bool>("IsProduction"))
                {
                    emailContract.BccReceivers.Add(manager.corporateEmail);
                }
            }

            var clientAttachments = new List<EmailAttachment>();

            if (request.Dossiers is not null && request.Dossiers.Count > 0)
            {
                foreach (var dossier in request.Dossiers)
                {
                    clientAttachments.Add(new EmailAttachment
                    {
                        Name = dossier.Split("/")[^1],
                        ContentType = "application/pdf",
                        DownloadUrl = dossier
                    });
                }
            }

            if (request.Attachments is not null && request.Attachments.Count > 0)
            {
                foreach (var clientAttachment in request.Attachments ?? new List<IFormFile>())
                {
                    var result = await _blobStorageService.UploadAsync(
                        clientAttachment.OpenReadStream(),
                        Guid.NewGuid().ToString(),
                        _connectionString,
                        _containerName);

                    if (result.Blob.Uri != null)
                    {
                        clientAttachments.Add(new EmailAttachment
                        {
                            Name = clientAttachment.FileName,
                            ContentType = clientAttachment.ContentType,
                            DownloadUrl = result.Blob.Uri
                        });
                    }
                }
            }

            emailContract = emailContract with
            {
                Attachments = clientAttachments
            };

            await _bus.Publish(emailContract, ct);

            action.Email = email;
            email.Action = action;

            await _context.SaveChangesAsync(ct);

            await SetProcessOngoing(request, ct);
            await SetColourToProcess(request.ProcessId, request.Colour, ct);
            await transaction.CommitAsync(ct);

            return email.Id;
        }
        catch (Exception)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }
    }

    private async Task SetProcessOngoing(EmailSendDto request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);

        if (process != null)
        {
            process.Status = ProcessStatus.Ongoing;
        }
    }

    private async Task SetColourToProcess(int processId, Colour colour, CancellationToken cancellationToken)
    {
        var processHasActiveCall = _context.Actions.Any(a =>
            a.ProcessId == processId && a.FinishDate == null && a.Type == ActionType.Call);

        if (!processHasActiveCall)
        {
            var process = await _context.Processes
                .Where(p => p.Id == processId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
            if (process is not null)
            {
                process.Colour = colour;
                await _processesService.UpdateProcess(process, cancellationToken);
            }
        }
    }
}
