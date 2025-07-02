using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetContactFaculties;

public class GetContactFacultiesQuery : IRequest<List<FacultyDto>>
{
    public int ContactId { get; set; }
}

public class GetContactFacultiesQueryHandler : IRequestHandler<GetContactFacultiesQuery, List<FacultyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContactFacultiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FacultyDto>> Handle(GetContactFacultiesQuery request, CancellationToken cancellationToken)
    {
        var facultyIds = await _context.ContactFaculty
            .Where(cf => cf.ContactId == request.ContactId && !cf.IsDeleted)
            .Select(f => f.FacultyId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Faculties
                .Where(f => facultyIds.Contains(f.Id) && !f.IsDeleted)
                .ProjectToListAsync<FacultyDto>(_mapper.ConfigurationProvider);
    }
}