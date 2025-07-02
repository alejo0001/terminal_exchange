using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Languages.Queries;

public class GetLanguagesQuery : IRequest<List<LanguageDto>>
{
}

public class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, List<LanguageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLanguagesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LanguageDto>> Handle(GetLanguagesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Languages
            .ProjectTo<LanguageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}