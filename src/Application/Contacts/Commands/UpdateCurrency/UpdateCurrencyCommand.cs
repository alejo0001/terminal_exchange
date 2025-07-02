using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateCurrency;

public class UpdateCurrencyCommand: IRequest
{
    public int CurrencyId { get; set; }
    public int ContactId { get; set; }        
}

public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateCurrencyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);

        contact!.CurrencyId = request.CurrencyId;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}