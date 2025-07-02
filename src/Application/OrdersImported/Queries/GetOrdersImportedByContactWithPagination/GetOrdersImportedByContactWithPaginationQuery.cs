using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.Common.Security;
using MediatR;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedByContactWithPagination;

[Authorize(Roles = "Usuario")]
public class GetOrdersImportedByContactWithPaginationQuery : IRequest<PaginatedList<OrdersImportedDto>>
{
    public int ContactId { get; set; } = 1;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string[] OrderBy { get; set; } = { "Id" };
    public string[] Order { get; set; } = { "asc" };
}
    
public class GetOrdersImportedByContactWithPaginationQueryHandler: IRequestHandler<GetOrdersImportedByContactWithPaginationQuery, PaginatedList<OrdersImportedDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
        
    public GetOrdersImportedByContactWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<PaginatedList<OrdersImportedDto>> Handle(GetOrdersImportedByContactWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var set = _context.OrdersImported
            .Where(o => o.ContactId == request.ContactId);
            
        if (!String.IsNullOrEmpty(request.QueryParams) && request.QueryParams != "\"\"")
        {
            set = set.ApplyParameters(_context, request.QueryParams);
        }
            
        return await set.ProjectTo<OrdersImportedDto>(_mapper.ConfigurationProvider)
            .OrderByDynamic(request.OrderBy, request.Order)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}