using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.RemoveContactSpecialities;

[Authorize(Roles = "Usuario")]
public class RemoveContactSpecialitiesCommand : IRequest
{
    public List<int> SpecialitiesId { get; set; }
    public int ContactId { get; set; }
}
    
public class RemoveContactSpecialitiesCommandHandler : IRequestHandler<RemoveContactSpecialitiesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPotentialsService _potentialsService;

    public RemoveContactSpecialitiesCommandHandler(IApplicationDbContext context, IPotentialsService potentialsService)
    {
        _context = context;
        _potentialsService = potentialsService;
    }
        
    public async Task<Unit> Handle(RemoveContactSpecialitiesCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .Include(c => c.Faculties)
            .Include(c => c.Specialities)
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        
        if (contact is  null)
        {
            throw new NotFoundException("Contact not found!");
        }

        foreach (var specialityId in request.SpecialitiesId)
        {
            var specialityToRemove = contact.Specialities.FirstOrDefault(f => f.Id == specialityId);
            if (specialityToRemove != null)
            {
                contact.Specialities.Remove(specialityToRemove);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken);
        return Unit.Value;
    }
}