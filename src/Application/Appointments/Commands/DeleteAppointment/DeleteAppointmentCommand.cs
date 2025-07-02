using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendar;

    public DeleteAppointmentCommandHandler(IApplicationDbContext context, ICalendarService calendar)
    {
        _context = context;
        _calendar = calendar;
    }
        
    public async Task<Unit> Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _context.Appointments.FindAsync(request.Id);
        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), request.Id);
        }

        await _calendar.DeleteEvent(appointment, cancellationToken).ConfigureAwait(false);

        appointment.IsDeleted = true;
        appointment.EventId = null;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}