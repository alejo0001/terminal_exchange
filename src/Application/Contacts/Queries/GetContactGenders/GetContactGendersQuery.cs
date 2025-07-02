using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetContactGenders;

public class GetContactGendersQuery : IRequest<List<ContactGenderDto>>
{
}

public class GetContactGendersQueryHandler : IRequestHandler<GetContactGendersQuery, List<ContactGenderDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetContactGendersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ContactGenderDto>> Handle(GetContactGendersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ContactGender
            .ProjectTo<ContactGenderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}