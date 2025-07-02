using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.DeleteContactLead;

public class DeleteContactLeadCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteContactCommandHandler : IRequestHandler<DeleteContactLeadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPotentialsService _potentialsService;

    public DeleteContactCommandHandler(IApplicationDbContext context, IPotentialsService potentialsService)
    {
        _context = context;
        _potentialsService = potentialsService;
    }
        
    public async Task<Unit> Handle(DeleteContactLeadCommand request, CancellationToken cancellationToken)
    {
        var contactLead = await _context.ContactLeads.FindAsync(request.Id);
        if (contactLead == null)
        {
            throw new NotFoundException(nameof(ContactLead), request.Id);
        }
        contactLead.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        var contactId = await _context.Contact
            .Where(c => c.Id == contactLead.ContactId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (contactId > 0)
        {
            await _potentialsService.CreateOrUpdateContactInPotentials(contactId, cancellationToken);
        }

        return Unit.Value;
    }
}