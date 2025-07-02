using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Faculties.Queries.GetCouponsOrigins;

public class GetCouponsOriginsQuery : IRequest<List<CouponsOriginsDto>>
{
}

public class GetCouponsOriginsQueryHandler : IRequestHandler<GetCouponsOriginsQuery, List<CouponsOriginsDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetCouponsOriginsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CouponsOriginsDto>> Handle(GetCouponsOriginsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.CouponOrigins
            .Where(co => !co.IsDeleted)
            .ProjectTo<CouponsOriginsDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}