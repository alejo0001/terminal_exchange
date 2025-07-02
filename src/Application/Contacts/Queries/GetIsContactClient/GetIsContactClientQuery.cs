using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetIsContactClient;

public class GetIsContactClientQuery: IRequest<bool>
{
    public int? ContactId { get; set; }
    public int? OriginContactId { get; set; }
}

public class GetIsContactClientQueryHandler : IRequestHandler<GetIsContactClientQuery, bool>
{
    private readonly ILeadsDbContext _iLeadsDbContext;
    private readonly IApplicationDbContext _context;

    public GetIsContactClientQueryHandler(ILeadsDbContext iLeadsDbContext, IApplicationDbContext context)
    {
        _iLeadsDbContext = iLeadsDbContext;
        _context = context;
    }

    public async Task<bool> Handle(GetIsContactClientQuery request, CancellationToken cancellationToken)
    {
        var isClient = false;

        var contact = await _context.Contact
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);

        if (contact is null)
        {
            contact = await _context.Contact
                .FirstOrDefaultAsync(c => c.OriginContactId == request.OriginContactId, cancellationToken);
        }

        if (contact is not null)
        {
            var orderImporteds = await _context.OrdersImported
                .Where(o => o.ContactId == contact.Id &&
                            !o.PaymentType.Contains("CANCELADO") &&
                            !o.PaymentType.Contains("pendiente"))
                .ToListAsync(cancellationToken);
            
            if (orderImporteds.Count > 0)
            {
                isClient = true;
            }
        }

        return isClient;
    }
}