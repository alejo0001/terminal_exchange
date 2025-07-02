using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.UpdateProcessColor;

public class UpdateProcessColorCommand: IRequest
{
    public int ProcessId { get; set; }
    public Colour Color { get; set; }
}
    
public class UpdateProcessColorCommandHandler : IRequestHandler<UpdateProcessColorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly ICalendarService _calendar;
    private readonly IProcessesService _processesService;

    public UpdateProcessColorCommandHandler(IApplicationDbContext context,
        IMapper mapper, ICurrentUserService currentUserService, 
        IDateTime dateTime, ICalendarService calendar, IProcessesService processesService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _calendar = calendar;
        _processesService = processesService;
    }

    public async Task<Unit> Handle(UpdateProcessColorCommand request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes.FindAsync(request.ProcessId);
        
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.Employee.CorporateEmail == _currentUserService.Email, cancellationToken);
        
        if (process == null)
        {
            throw new NotFoundException(nameof(Process), request.ProcessId);
        }

        process.Colour = request.Color;
        
        var newAction = new Action
        {
            UserId = user!.Id,
            ContactId = process.ContactId,
            User = user,
            Date = _dateTime.Now,
            FinishDate = _dateTime.Now,
            ProcessId = process.Id,
            Type = ActionType.ChangeColor,
            Outcome = ActionOutcome.ChangedColor
        };

        _context.Actions.Add(newAction);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}