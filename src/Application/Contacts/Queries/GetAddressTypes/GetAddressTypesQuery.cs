using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetAddressTypes;

public class GetAddressTypesQuery : IRequest<List<AddressTypeDto>>
{
}

public class GetAddressTypesQueryHandler : IRequestHandler<GetAddressTypesQuery, List<AddressTypeDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetAddressTypesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AddressTypeDto>> Handle(GetAddressTypesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.AddressType
            .ProjectTo<AddressTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}