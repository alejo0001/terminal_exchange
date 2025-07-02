using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;

namespace CrmAPI.Application.Actions.Queries.GetActionCallActive;

public class GetActionCallActiveQuery : IRequest<bool>
{
    public int ProcessId { get; set; }
}

public class GetActionCallActiveQueryHandler : IRequestHandler<GetActionCallActiveQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public GetActionCallActiveQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> Handle(GetActionCallActiveQuery request, CancellationToken cancellationToken)
    {
        return  Task.FromResult(_context.Actions.Any(a=>a.ProcessId == request.ProcessId && a.FinishDate == null && a.Type == ActionType.Call));
    }
}