using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.AddContactFaculties;

[Authorize(Roles = "Usuario")]
public class AddContactFacultiesCommand : ContactFacultiesCreateDto, IRequest<int>
{
}
    
public class AddContactFacultiesCommandHandler : IRequestHandler<AddContactFacultiesCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPotentialsService _potentialsService;

    public AddContactFacultiesCommandHandler(IApplicationDbContext context, IPotentialsService potentialsService)
    {
        _context = context;
        _potentialsService = potentialsService;
    }
        
    public async Task<int> Handle(AddContactFacultiesCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .Include(c => c.Faculties)
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);

        var falculties = await _context.Faculties
            .Where(f => request.FacultiesId.Contains(f.Id))
            .ToListAsync(cancellationToken);
        contact!.Faculties.AddRange(falculties);

        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken);
        
        return contact.Id;
    }
}