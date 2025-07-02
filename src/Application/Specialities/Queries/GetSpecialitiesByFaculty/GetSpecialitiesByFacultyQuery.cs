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

namespace CrmAPI.Application.Specialities.Queries.GetSpecialitiesByFaculty;

public class GetSpecialitiesByFacultyQuery : IRequest<List<SpecialityDto>>
{
    public int FacultyId { get; set; }
    public int CourseCountryId { get; set; }
}

public class GetSpecialitiesByFacultyQueryHandler : IRequestHandler<GetSpecialitiesByFacultyQuery, List<SpecialityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSpecialitiesByFacultyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SpecialityDto>> Handle(GetSpecialitiesByFacultyQuery request,
        CancellationToken cancellationToken)
    {
        var set = _context.FacultySpecialities
            .Include(fs => fs.Speciality)
            .Include(fs => fs.Faculty)
            .ThenInclude(f => f.CountryFaculties)
            .Where(fs => fs.FacultyId == request.FacultyId
                         && fs.Faculty.CountryFaculties.Any(cf => cf.CourseCountryId == request.CourseCountryId))
            .Select(fs => fs.Speciality)
            .AsQueryable();
        return await set.ProjectTo<SpecialityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}