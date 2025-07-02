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

namespace CrmAPI.Application.DiscardReasons.Queries.GetAllDiscardReasons;

public class GetAllDiscardReasonsQuery : IRequest<List<DiscardReasonDto>>
{
}

public class GetAllDiscardReasonsQueryHandler : IRequestHandler<GetAllDiscardReasonsQuery, List<DiscardReasonDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllDiscardReasonsQueryHandler(IMapper mapper, IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    
    public async Task<List<DiscardReasonDto>> Handle(GetAllDiscardReasonsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.DiscardReasons
            .Where(dr => !dr.IsDeleted)
            .ProjectTo<DiscardReasonDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}