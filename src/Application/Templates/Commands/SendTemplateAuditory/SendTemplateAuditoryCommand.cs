using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using CrmAPI.Application.Settings;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationAPI.Contracts.Commands;
using EmailAttachment = NotificationAPI.Contracts.Common.EmailAttachment;
using Uri = System.Uri;

namespace CrmAPI.Application.Templates.Commands.SendTemplateAuditory;

[Authorize(Roles = "Auditor")]
public record SendTemplateAuditoryCommand : IRequest<Unit>
{
}

[UsedImplicitly]
public class SendTemplateByEmailCommandHandler : IRequestHandler<SendTemplateAuditoryCommand, Unit>
{
    
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SendTemplateByEmailCommandHandler> _logger;
    private readonly DataBlobAuditoryConnectionSettings _settings;
    
    public SendTemplateByEmailCommandHandler( 
        ICurrentUserService currentUserService, 
        IConfiguration configuration, 
        IPublishEndpoint publishEndpoint,
        IOptionsSnapshot<DataBlobAuditoryConnectionSettings> options,
        ILogger<SendTemplateByEmailCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _settings = options.Value;
    }
    
    public async Task<Unit> Handle(SendTemplateAuditoryCommand request, CancellationToken ct)
    {
        try
        {
            var uri = new Uri(
                $"{_settings.BaseUriAuditory}/" +
                $"{_settings.ContainerNameAuditory}/" +
                $"{_settings.FilenameAuditory}/" +
                $"{_settings.AccessTokenAuditory}");

            var email = CreateFileEmail(uri.ToString());
            
            await _publishEndpoint.Publish(email, ct);
        }
        catch (Exception ex)
        {
            // Log error to console and create a new response we can return to the requesting method
            _logger.LogError(ex, "Unhandled exception occurred. Error Message: {ErrorMessage}, " +
                                 "StackTrace ID: {StackTraceID}", ex.Message, ex.StackTrace);
        }
        return Unit.Value;
    }
    
    private CreateEmail CreateFileEmail(string uri)
    {
        var email = new CreateEmail()
        {
            CorrelationId = Guid.NewGuid(),
            Subject = "Download Template of auditory",
            Body = "If you encounter any problems, please contact <a href=\"mailto:soporte@techtitute.com\">soporte@techtitute.com</a>",
            From = _configuration["Emails:Intranet"] ?? "Intranet",
            Receivers = new List<string> {_currentUserService.Email},
            Attachments = new List<EmailAttachment>
            {
                new()
                {
                    Name = _settings.FilenameAuditory,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    DownloadUrl = uri
                }
            },
            UseExchange = false
        };
        return email;
    }
}