using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateContactLeadPrice;

public class UpdateContactLeadPriceCommand: ContactLeadPriceUpdateDto, IRequest<int>
{
}

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactLeadPriceCommand, int>
{
    private readonly IApplicationDbContext _context;

    public UpdateContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(UpdateContactLeadPriceCommand request, CancellationToken cancellationToken)
    {
        var contactLead = await _context.ContactLeads
            .FirstOrDefaultAsync(cl => cl.Id == request.ContactLeadId, cancellationToken);
            
        if (contactLead == null)
        {
            return 0;
        }

        contactLead.Discount = request.Discount ?? contactLead.Discount;
        contactLead.Price = request.Price ?? contactLead.Price;
        contactLead.FinalPrice = request.FinalPrice ?? contactLead.FinalPrice;
        contactLead.EnrollmentPercentage = request.EnrollmentPercentage ?? contactLead.EnrollmentPercentage;
        contactLead.Fees = request.Fees ?? contactLead.Fees;
        
        return await _context.SaveChangesAsync(cancellationToken);
    }
}