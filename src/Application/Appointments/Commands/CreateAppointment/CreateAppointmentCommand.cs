using System.Collections.Generic;
using System.Linq;
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

namespace CrmAPI.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommand : AppointmentCreateDto, IRequest<int>
{
}
    
public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;
    private readonly ICalendarService _calendar;

    public CreateAppointmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IMapper mapper, IDateTime dateTime, ICalendarService calendar)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _dateTime = dateTime;
        _calendar = calendar;
    }
        
    public async Task<int> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = _mapper.Map<Appointment>(request);

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.Employee.CorporateEmail == _currentUserService.Email, cancellationToken);

        if (user == null) throw new ForbiddenAccessException();
            
        appointment.UserId = user.Id;
            
        var contact =
            await _context.Contact
                .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        contact!.NextInteraction = appointment.Date;
        _context.Appointments.Add(appointment);
            
        List<Appointment> oldAppointments = await _context.Appointments
            .Include(a => a.Contact)
            .Where(a => a.ProcessId == request.ProcessId && a.ContactId == request.ContactId && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        bool isEventUpdate = false;
            
        // Si hay algùn appointment anterior se establece su EventId a null
        // (en el calendario solo habrá un evento por appointment)
        if (oldAppointments.Any())
        {
            appointment.EventId = oldAppointments.FirstOrDefault(a => !a.IsDeleted && a.EventId != null)?.EventId;
                
            //Actualizar cita en el calendario
            if (appointment.EventId != null)
            {
                isEventUpdate = true;
                appointment.EventId = await _calendar.UpdateEvent(appointment, cancellationToken).ConfigureAwait(false);
            }

            oldAppointments.ForEach(a =>
            {
                a.IsDeleted = true;
                a.EventId = null; // desvincular evento en el calendario
            });
        } 

        if(!isEventUpdate)
            appointment.EventId = await _calendar.CreateEvent(appointment, cancellationToken).ConfigureAwait(false);

        appointment.ActionId = await CreateNewAction(request, user, appointment, cancellationToken);

        // Actualizamos el proceso a "en progreso" cuando se crea una cita.
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);
        process!.Status = ProcessStatus.Ongoing;
        process.SentReminderNotification = false;
            
        await _context.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }

    private async Task<int> CreateNewAction(CreateAppointmentCommand request, User user, Appointment appointment, CancellationToken cancellationToken)
    {
        Action newAction  = new()
        {
            UserId = user.Id,
            ContactId = request.ContactId,
            User = user,
            Date = _dateTime.Now,
            FinishDate = _dateTime.Now,
            ProcessId = request.ProcessId,
            Type = ActionType.Appointment,
            Outcome = ActionOutcome.Appointment,
            Appointments = new List<Appointment>{appointment}
        };
        _context.Actions.Add(newAction);
        await _context.SaveChangesAsync(cancellationToken);

        return newAction.Id;
    }

        
}