using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedDetails;

public class GetOrdersImportedDetailsQuery : IRequest<OrdersImportedDto>
{
    public int Id { get; set; }
}
    
public class GetOrdersImportedDetailsQueryHandler : IRequestHandler<GetOrdersImportedDetailsQuery, OrdersImportedDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
        
    public GetOrdersImportedDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<OrdersImportedDto> Handle(GetOrdersImportedDetailsQuery request, CancellationToken cancellationToken)
    {
        var process = await _context.OrdersImported
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (process == null)
        {
            throw new NotFoundException(nameof(OrdersImported), request.Id);
        }
        return _mapper.Map<OrdersImportedDto>(process);
    }
}