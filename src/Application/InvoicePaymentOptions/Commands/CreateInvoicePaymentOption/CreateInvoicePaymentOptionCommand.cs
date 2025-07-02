using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.InvoicePaymentOptions.Commands.CreateInvoicePaymentOption;

public class CreateInvoicePaymentOptionCommand: InvoicePaymentOptionCreateDto, IRequest<int>
{
}
    
public class CreateInvoicePaymentOptionCommandHandler : IRequestHandler<CreateInvoicePaymentOptionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateInvoicePaymentOptionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<int> Handle(CreateInvoicePaymentOptionCommand request, CancellationToken cancellationToken)
    {
        InvoicePaymentOption option = _mapper.Map<InvoicePaymentOption>(request);
        _context.InvoicePaymentOptions.Add(option);
        await _context.SaveChangesAsync(cancellationToken);
        return option.Id;
    }
}