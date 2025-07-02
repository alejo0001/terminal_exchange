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

namespace CrmAPI.Application.Specialities.Queries.GetSpecialitiesByFaculties;

public class GetSpecialitiesByFacultiesQuery : IRequest<List<SpecialityDto>>
{
    public List<int> FacultiesId { get; set; }
    public int CourseCountryId { get; set; }
}

public class GetSpecialitiesByFacultiesQueryHandler : IRequestHandler<GetSpecialitiesByFacultiesQuery, List<SpecialityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSpecialitiesByFacultiesQueryHandler(IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SpecialityDto>> Handle(GetSpecialitiesByFacultiesQuery request,
        CancellationToken cancellationToken)
    {

        var set = _context.FacultySpecialities
            .Include(fs => fs.Speciality)
            .Include(fs => fs.Faculty)
            .ThenInclude(f => f.CountryFaculties)
            .Where(fs => request.FacultiesId.Contains(fs.FacultyId)
                         && fs.Faculty.CountryFaculties.Any(cf => cf.CourseCountryId == request.CourseCountryId))
            .Select(fs => fs.Speciality)
            .AsQueryable();

        return (await set.ProjectTo<SpecialityDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken))
            .GroupBy(s => s.Id).Select(g => g.First()).ToList();
    }
}