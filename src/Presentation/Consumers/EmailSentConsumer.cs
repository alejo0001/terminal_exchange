using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Events;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public partial class EmailSentConsumer : IConsumer<EmailSent>
{
    private readonly IApplicationDbContext _context;
    private readonly IActionsService _actionsService;
    private readonly IProcessesService _processesService;
    private readonly ILogger<EmailSentConsumer> _logger;

    public EmailSentConsumer(
        IApplicationDbContext context,
        IActionsService actionsService,
        IProcessesService processesService,
        ILogger<EmailSentConsumer> logger)
    {
        _context = context;
        _actionsService = actionsService;
        _processesService = processesService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailSent> context)
    {
        var correlationId = context.Message.CorrelationId;

        LogTraceEmailSentEvent(_logger, correlationId, ActionType.EmailSucceeded);

        var action = await _context.Actions.FirstOrDefaultAsync(
            a => a.Guid == correlationId,
            CancellationToken.None);

        if (action == null)
        {
            return;
        }

        action.Type = ActionType.EmailSucceeded;
        await _actionsService.UpdateAction(action, CancellationToken.None);
        await _processesService.SetIfIsFirstInteractionDateInProcess(action, CancellationToken.None);
    }

    [LoggerMessage(1, LogLevel.Trace, "EmailSent event received for {CorrelationId}, set Action to {ActionType}")]
    private static partial void LogTraceEmailSentEvent(ILogger l, Guid correlationId, ActionType actionType);
}
