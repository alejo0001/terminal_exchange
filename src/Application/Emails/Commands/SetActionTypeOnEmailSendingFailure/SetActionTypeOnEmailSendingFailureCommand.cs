using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using ErrorOr;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Emails.Commands.SetActionTypeOnEmailSendingFailure;

/// <summary>
///     Sets corresponding Action's <see cref="IntranetMigrator.Domain.Entities.Action.Type" /> to
///     <see cref="SetActionTypeOnEmailSendingFailureCommandHandler.FailureType" />, updates <c>LastModified*</c> fields
///     and logs with <see cref="LogLevel.Trace" />.
/// </summary>
/// <remarks>Created to react to the Failure/Fault Events in NotificationApi.</remarks>
[UsedImplicitly]
public record SetActionTypeOnEmailSendingFailureCommand(Guid CorrelationId, IEnumerable<string> FailedEventNames)
    : IRequest<ErrorOr<Updated>>;

[UsedImplicitly]
public partial class SetActionTypeOnEmailSendingFailureCommandHandler
    : IRequestHandler<SetActionTypeOnEmailSendingFailureCommand, ErrorOr<Updated>>
{
    private const ActionType FailureType = ActionType.EmailFailed;

    private static readonly string ModifyingActor;

    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SetActionTypeOnEmailSendingFailureCommandHandler> _logger;

    public SetActionTypeOnEmailSendingFailureCommandHandler(
        IApplicationDbContext dbContext,
        ILogger<SetActionTypeOnEmailSendingFailureCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    static SetActionTypeOnEmailSendingFailureCommandHandler() =>
        ModifyingActor = PersistenceAuditingHelper.InjectCreatorPartialAsTag(
            string.Empty,
            nameof(SetActionTypeOnEmailSendingFailureCommandHandler));

    public async Task<ErrorOr<Updated>> Handle(SetActionTypeOnEmailSendingFailureCommand request, CancellationToken ct)
    {
        LogTraceEmailFailureEvent(request.FailedEventNames, request.CorrelationId);

        await _dbContext.Actions
            .Where(a => a.Guid == request.CorrelationId)
            .ExecuteUpdateAsync(
                x => x.SetProperty(a => a.Type, FailureType)
                    .SetProperty(a => a.LastModified, DateTime.UtcNow)
                    .SetProperty(a => a.LastModifiedBy, ModifyingActor),
                ct);

        return Result.Updated;
    }

    private void LogTraceEmailFailureEvent(IEnumerable<string> failedEventNames, Guid correlationId)
    {
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        var sb = new StringBuilder().AppendJoin(", ", failedEventNames);

        LogTraceEmailFailureEvent(_logger, sb, correlationId, FailureType);
    }

    [LoggerMessage(
        1,
        LogLevel.Trace,
        "Received [{EmailFailureEvents}] with {CorrelationId} because of an error in NotificationApi; "
        + "set corresponding Action entity to {ActionType}")]
    private static partial void LogTraceEmailFailureEvent(
        ILogger l,
        StringBuilder emailFailureEvents,
        Guid correlationId,
        ActionType actionType);
}
