using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.DeleteContact;

[Authorize(Roles = "Usuario")]
public class DeleteContactCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendar;
        

    public DeleteContactCommandHandler(IApplicationDbContext context, ICalendarService calendar)
    {
        _context = context;
        _calendar = calendar;
    }
        
    public async Task<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {

        Contact contact = await _context.Contact
            .Include(c => c.Processes)
            .Include(c => c.Appointments.Where(a=> !a.IsDeleted))
            .Include(c => c.Annotations)
            .Include(c => c.ContactPhone)
            .Include(c => c.ContactEmail)
            .Include(c => c.ContactTitles)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException(nameof(Contact), request.Id);
        }

        contact.IsDeleted = true;

        foreach (var process in contact.Processes)
        {
            process.Status = ProcessStatus.Closed;
        }

        await _calendar.DeleteAllContactEvents(contact, cancellationToken).ConfigureAwait(false);

        foreach (var annotation in contact.Annotations)
        {
            annotation.IsDeleted = true;
        }
            
        foreach (var contactPhone in contact.ContactPhone)
        {
            contactPhone.IsDeleted = true;
        }
            
        foreach (var contactEmail in contact.ContactEmail)
        {
            contactEmail.IsDeleted = true;
        }
            
        foreach (var contactTitle in contact.ContactTitles)
        {
            contactTitle.IsDeleted = true;
        }

        contact.NextInteraction = null;
            
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}