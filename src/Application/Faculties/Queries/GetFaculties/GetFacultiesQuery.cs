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

namespace CrmAPI.Application.Faculties.Queries.GetFaculties;

public class GetFacultiesQuery : IRequest<List<FacultyDto>>
{
}

public class GetFacultiesQueryHandler : IRequestHandler<GetFacultiesQuery, List<FacultyDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;
    private readonly ITlmkDbContext _tlmkDbContext;

    public GetFacultiesQueryHandler(IApplicationDbContext context, IMapper mapper, ITlmkDbContext tlmkDbContext)
    {
        _context = context;
        _mapper = mapper;
        _tlmkDbContext = tlmkDbContext;
    }

    public async Task<List<FacultyDto>> Handle(GetFacultiesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(f => f.Name != null && !f.IsDeleted)
            .ProjectTo<FacultyDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}