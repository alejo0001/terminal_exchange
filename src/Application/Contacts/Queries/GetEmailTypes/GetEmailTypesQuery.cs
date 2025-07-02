using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetEmailTypes;

public class GetEmailTypesQuery : IRequest<List<EmailTypeDto>>
{
}

public class GetEmailTypesQueryHandler : IRequestHandler<GetEmailTypesQuery, List<EmailTypeDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetEmailTypesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EmailTypeDto>> Handle(GetEmailTypesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.EmailType
            .ProjectTo<EmailTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}