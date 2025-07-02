using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Currencies.Queries.GetCurrencyByCode;

public class GetCurrencyByCodeQuery : IRequest<CurrencyDto>
{
    public string CurrencyCode { get; set; }
}

public class GetCurrencyByCodeQueryHandler : IRequestHandler<GetCurrencyByCodeQuery, CurrencyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCurrencyByCodeQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CurrencyDto> Handle(GetCurrencyByCodeQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Currency
            .Where(c => c.CurrencyCode == request.CurrencyCode)
            .ProjectTo<CurrencyDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}