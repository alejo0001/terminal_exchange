using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetPhoneTypes;

public class GetPhoneTypesQuery : IRequest<List<PhoneTypeDto>>
{
}

public class GetPhoneTypesQueryHandler : IRequestHandler<GetPhoneTypesQuery, List<PhoneTypeDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetPhoneTypesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PhoneTypeDto>> Handle(GetPhoneTypesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PhoneType
            .ProjectTo<PhoneTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}