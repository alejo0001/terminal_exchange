using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.InvoicePaymentOptions.Commands.DeleteInvoicePaymentOption;

public class DeleteInvoicePaymentOptionCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteInvoicePaymentOptionCommandHandler : IRequestHandler<DeleteInvoicePaymentOptionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteInvoicePaymentOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
        
    public async Task<Unit> Handle(DeleteInvoicePaymentOptionCommand request, CancellationToken cancellationToken)
    {
        InvoicePaymentOption option = await _context.InvoicePaymentOptions.FindAsync(request.Id);
        if (option == null)
        {
            throw new NotFoundException(nameof(InvoicePaymentOption), request.Id);
        }
        option.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}