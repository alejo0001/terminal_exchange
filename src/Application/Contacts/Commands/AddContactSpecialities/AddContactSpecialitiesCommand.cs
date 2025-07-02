using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.AddContactSpecialities;

[Authorize(Roles = "Usuario")]
public class AddContactSpecialitiesCommand : ContactSpecialitiesCreateDto, IRequest<int>
{
}
    
public class AddContactSpecialitiesCommandHandler : IRequestHandler<AddContactSpecialitiesCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPotentialsService _potentialsService;

    public AddContactSpecialitiesCommandHandler(IApplicationDbContext context, IPotentialsService potentialsService)
    {
        _context = context;
        _potentialsService = potentialsService;
    }
        
    public async Task<int> Handle(AddContactSpecialitiesCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .Include(c => c.Specialities)
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        
        if (contact == null)
        {
            throw new NotFoundException("Contact not found!");
        }

        var specialities = await _context.Specialities
            .Where(s => request.SpecialitiesId.Contains(s.Id))
            .ToListAsync(cancellationToken);
        contact.Specialities.AddRange(specialities);
            
        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken);
        
        return contact.Id;
    }
}