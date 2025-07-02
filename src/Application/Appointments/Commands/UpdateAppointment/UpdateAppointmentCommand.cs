using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommand : AppointmentUpdateDto, IRequest
{
}
    
public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendar;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context, ICalendarService calendar)
    {
        _context = context;
        _calendar = calendar;
    }
        
    public async Task<Unit> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Contact)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), request.Id);
        }

        appointment.Date = request.Date;
        appointment.UserId = request.UserId;
        appointment.Title = request.Title;
        appointment.Contact.NextInteraction = request.Date;

        await _calendar.UpdateEvent(appointment, cancellationToken).ConfigureAwait(false);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}