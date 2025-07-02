using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Actions.Commands.UpdateAction;

public class UpdateActionCommand: ActionUpdateDto, IRequest
{
}

public class UpdateActionCommandHandler : IRequestHandler<UpdateActionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;
    private readonly IConfiguration _configuration;
    private readonly IWorkingDays _workingDaysService;
    private readonly IProcessesService _processesService;
    private readonly ICalendarService _calendar;

    public UpdateActionCommandHandler(IApplicationDbContext context, IMapper mapper,
        IWorkingDays workingDaysService,
        IDateTime dateTime, IConfiguration configuration, IProcessesService processesService,
        ICalendarService calendar)
    {
        _context = context;
        _mapper = mapper;
        _dateTime = dateTime;
        _configuration = configuration;
        _processesService = processesService;
        _calendar = calendar;
        _workingDaysService = workingDaysService;
    }

    public async Task<Unit> Handle(UpdateActionCommand request, CancellationToken cancellationToken)
    {
        var action = await UpdateAction(request);

        var process = await _context.Processes
            .Where(p => p.Id == request.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);

        // No sé pq no se puede comprobar en el validador, da error
        if (process is null)
        {
            return Unit.Value;
        }

        // COMPROBAMOS SI EL CONTACTO TIENE ALGUNA CITA QUE NO ESTE CADUCADA 
        var appointment = await _context.Appointments
            .Where(c => c.ContactId == request.ContactId
                        && c.ProcessId == request.ProcessId
                        && c.Date > _dateTime.Now)
            .FirstOrDefaultAsync(cancellationToken);

        var appoimentExpired = appointment != null && appointment.Date.Date <= _dateTime.Now.Date;
        
        if (appoimentExpired)
        {
            appointment.IsDeleted = true;
            await _calendar.DeleteEvent(appointment, cancellationToken).ConfigureAwait(false);
        }
        

        if (request.ActionType == ActionType.Call.ToString().ToLowerInvariant() &&
            request.Outcome == ActionOutcome.NoResponse.ToString().ToLowerInvariant()
            && appoimentExpired
           )
        {
            var contact = await _context.Contact.FindAsync(request.ContactId);
            if (contact is not null)
            {
                var nextInteraction = await _processesService.GetNextInteractionDateWhenThereIsNoResponse(process, request.Date, cancellationToken);
                contact.NextInteraction = nextInteraction;
            }
        }

        await _processesService.SetColourProcess(process, action, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Action> UpdateAction(UpdateActionCommand request)
    {
        var actionAux = _mapper.Map<Action>(request);
        var action = await _context.Actions.FindAsync(request.Id);
        if (action == null)
        {
            throw new NotFoundException(nameof(Action), request.Id);
        }
        
        if (request.UserId != null)
            action.UserId = request.UserId;

        if (request.ContactId > 0)
            action.ContactId = request.ContactId;
            
        if (request.ProcessId is > 0)
            action.ContactId = request.ContactId;

        if (request.OrdersImportedId is > 0)
            action.OrdersImportedId = request.OrdersImportedId;

        action.FinishDate = request.FinishDate;

        PropertyInfo[] properties = action.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            var value = property.GetValue(actionAux, null);
            if (value != null && (property.Name.Equals("Type") || property.Name.Equals("Outcome")))
            {
                action.GetType().GetProperty(property.Name)?.SetValue(action, value, null);
            }
        }
        return action;
    }

}