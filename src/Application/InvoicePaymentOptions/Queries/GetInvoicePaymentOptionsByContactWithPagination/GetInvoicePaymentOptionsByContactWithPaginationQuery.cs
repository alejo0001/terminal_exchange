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
using MediatR;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.InvoicePaymentOptions.Queries.GetInvoicePaymentOptionsByContactWithPagination;

public class GetInvoicePaymentOptionsByContactWithPaginationQuery: IRequest<PaginatedList<InvoicePaymentOptionDto>>
{
    public int ContactId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string[] OrderBy { get; set; } = { "Id" };
    public string[] Order { get; set; } = { "asc" };

}
    
public class GetContactsByUserWithPaginationQueryHandler : IRequestHandler<GetInvoicePaymentOptionsByContactWithPaginationQuery, PaginatedList<InvoicePaymentOptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
        
    public GetContactsByUserWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<PaginatedList<InvoicePaymentOptionDto>> Handle(GetInvoicePaymentOptionsByContactWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var set = _context.InvoicePaymentOptions
            .Where(c => !c.IsDeleted)
            .Join(
                _context.ContactInvoicePaymentOption.Where(u => u.ContactId == request.ContactId),
                a => a.Id,
                b => b.InvoicePaymentOptionId,
                (a, b) => a
            )
            .AsQueryable();
        if (!String.IsNullOrEmpty(request.QueryParams) && request.QueryParams != "\"\"")
        {
            set = set.ApplyParameters(_context, request.QueryParams);
        }
        set = set.OrderByDynamic(request.OrderBy, request.Order);
        var aux = await set.ProjectTo<InvoicePaymentOptionDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
        foreach (var option in aux.Items)
        {
            option.HasNonPayment = _context.OrdersImported.Where(o => o.CreditCardNumber == option.Number)
                .Join(
                    _context.Processes.Where(p => !p.IsDeleted),
                    a => a.ProcessId,
                    b => b.Id,
                    (a, b) => b
                ).Any();
            option.Number = "**** **** **** " + option.Number.Substring(option.Number.Length - 4);
        }
        return aux;
    }
}