using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Queries.GetActionsSendEmailDayZero;

/// <summary>
///     Check, whether <see cref="IntranetMigrator.Domain.Entities.Action" /> has it's <see cref="Action.Type" />
///     one of <see cref="ActionType.EmailPending" />, <see cref="ActionType.EmailSucceeded" /> or
///     <see cref="ActionType.EmailFailed" />.
/// </summary>
/// <param name="ProcessId"></param>
[Authorize]
public record CheckHasSentEmailsQuery(int ProcessId) : IRequest<bool>;

[UsedImplicitly]
public class CheckHasSentEmailsQueryHandler : IRequestHandler<CheckHasSentEmailsQuery, bool>
{
    private readonly IApplicationDbContext _context;
    public CheckHasSentEmailsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(CheckHasSentEmailsQuery request, CancellationToken ct) =>
        await _context.Actions
            .Where(x => x.ProcessId == request.ProcessId)
            .Where(x => !x.IsDeleted)
            .Where(x => new[] { ActionType.EmailPending, ActionType.EmailSucceeded, ActionType.EmailFailed }
                .Contains(x.Type))
            .AnyAsync(ct).ConfigureAwait(false);
}
