using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Countries.Queries.GetCountries;

public class GetCountriesQuery: IRequest<List<CountryDto>>
{
}

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, List<CountryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCountriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<List<CountryDto>> Handle(GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Country
            .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}