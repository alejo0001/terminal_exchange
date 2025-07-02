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

namespace CrmAPI.Application.Contacts.Queries.GetContactSpecialities;

public class GetContactSpecialitiesQuery : IRequest<List<SpecialityDto>>
{
    public int ContactId { get; set; }
}

public class GetContactSpecialitiesQueryHandler : IRequestHandler<GetContactSpecialitiesQuery, List<SpecialityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContactSpecialitiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SpecialityDto>> Handle(GetContactSpecialitiesQuery request, CancellationToken cancellationToken)
    {
        var specialityIds  = await _context.ContactSpeciality
            .Where(cs => cs.ContactId == request.ContactId && !cs.IsDeleted)
            .Select(f => f.SpecialityId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Specialities
                .Where(s => specialityIds.Contains(s.Id) && !s.IsDeleted)
                .ProjectToListAsync<SpecialityDto>(_mapper.ConfigurationProvider);
    }
}