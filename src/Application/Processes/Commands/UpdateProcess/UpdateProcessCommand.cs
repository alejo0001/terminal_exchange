using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Processes.Commands.UpdateProcess;

public class UpdateProcessCommand: ProcessUpdateDto, IRequest
{
}
    
public class UpdateProcessCommandHandler : IRequestHandler<UpdateProcessCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IProcessesService _processesService;
    private readonly ICalendarService _calendar;
    private readonly ICloseProcessService _closeProcessService;
    private readonly IPotentialsService _potentialsService;

    public UpdateProcessCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ICloseProcessService closeProcessService,
        IDateTime dateTime,
        IProcessesService processesService,
        ICalendarService calendar, 
        IPotentialsService potentialsService)
    {
        _context = context;
        _mapper = mapper;
        _closeProcessService = closeProcessService;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _processesService = processesService;
        _calendar = calendar;
        _potentialsService = potentialsService;
    }

    public async Task<Unit> Handle(UpdateProcessCommand request, CancellationToken ct)
    {
        var process = await _processesService.UpdateProcessFromProcess(_mapper.Map<Process>(request), ct);
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Employee.CorporateEmail == _currentUserService.Email, ct);

        if(request.Outcome.Equals(ProcessOutcome.NotSale.ToString().ToLowerInvariant()) 
           && request.Status.Equals(ProcessStatus.Closed.ToString().ToLowerInvariant()))
        {
            var newAction = new Action
            {
                UserId = user!.Id,
                ContactId = process.ContactId,
                User = user,
                Date = _dateTime.Now,
                FinishDate = _dateTime.Now,
                ProcessId = process.Id,
                Type = ActionType.Discard,
                Outcome = ActionOutcome.NotInterested
            };

            var discardReasonProcess = new DiscardReasonProcess
            {
                ProcessId = process.Id,
                DiscardReasonId = request.DiscardReasonId ?? 1,
                Observations = request.DiscardReasonObservations ?? ""
            };

            List<Process> processAsList = new List<Process>();
            processAsList.Add(process);

            await _potentialsService.CreateOrUpdateContactInPotentials(request.ContactId, ct);
            await _processesService.OnCloseProcessActions(request.Id, process, newAction, discardReasonProcess, ct);
            await _closeProcessService.EmailCloseProcessStepByStep(processAsList, ct);

            if (process.Contact.NextInteraction is null
                && process.Status is ProcessStatus.Pending or ProcessStatus.Closed
                && process.Contact.Appointments is not null 
                && process.Contact.Appointments.Any(a => !a.IsDeleted))
            {
                await _calendar.DeleteAllContactEvents(process.Contact, ct).ConfigureAwait(false);
            }
        }
        
        if (request.Status.Equals(ProcessStatus.Pending.ToString().ToLowerInvariant()))
        {
            var newAction = new Action
            {
                UserId = user!.Id,
                ContactId = process.ContactId,
                User = user,
                Date = _dateTime.Now,
                FinishDate = _dateTime.Now,
                ProcessId = process.Id,
                Type = ActionType.Sale,
                Outcome = ActionOutcome.Sale
            };
            //TODO: COMPROBAR QUE TAL VEZ ESTO DEBERÍA IR EN EL MOMENTO DE CONFIRMAR EL COBRO.
            await _processesService.OnCloseProcessActions(request.Id, process, newAction, null, ct);
        }

        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}