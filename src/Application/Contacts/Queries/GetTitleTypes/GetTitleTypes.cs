using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetTitleTypes;

public class GetTitleTypesQuery : IRequest<List<TitleTypeDto>>
{
}
    
public class GetTitleTypesQueryHandler : IRequestHandler<GetTitleTypesQuery, List<TitleTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTitleTypesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TitleTypeDto>> Handle(GetTitleTypesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.TitleTypes
            .ProjectTo<TitleTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}