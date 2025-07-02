using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateCountryCode;

public class UpdateCountryCodeCommand: IRequest
{
    public string CountryCode { get; set; }
    public int ContactId { get; set; }
}

public class UpdateCountryCodeCommandHandler : IRequestHandler<UpdateCountryCodeCommand, Unit>
{
        
    private readonly IApplicationDbContext _context;

    public UpdateCountryCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCountryCodeCommand request, CancellationToken cancellationToken)
    {
            
        var contact = await _context.Contact
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
            
        if (contact == null)
        {
            throw new NotFoundException(nameof(Contact), request.ContactId);
        }

        contact.CountryCode = request.CountryCode;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}