using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Queries.GetActionCallInfo;

public class GetActionCallInfoQuery : IRequest<ActionInfoDto>
{
    public int ProcessId { get; set; }
}

public class GetActionCallInfoQueryHandler : IRequestHandler<GetActionCallInfoQuery, ActionInfoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;
    private readonly IDateTime _dateTime;

    public GetActionCallInfoQueryHandler(IApplicationDbContext context, IProcessesService processesService, IDateTime dateTime)
    {
        _context = context;
        _processesService = processesService;
        _dateTime = dateTime;
    }

    public async Task<ActionInfoDto> Handle(GetActionCallInfoQuery request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .Include(p => p.Actions)
            .Include(p => p.User.Employee)
            .Include(p => p.Appointments
                .Where(a => a.Date.Date >= _dateTime.Now.Date))
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);

        if (process == null)
        {
            throw new NotFoundException("Process not found!");
        }

        return new ActionInfoDto
        {
            Day = await _processesService.GetProcessDay(process),
            AttemptsCalls = await _processesService.ComputeAttempts(process.Actions, process.Type),
            MaxAttemptsCalls = await _processesService.GetTotalTriesCallByDay(process.Type, cancellationToken)
        };
    }
}