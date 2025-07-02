using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using MediatR;

namespace CrmAPI.Application.Contacts.Queries.GetContactEmails;

public class GetContactEmailsQuery : IRequest<List<ContactEmailDto>>
{
    public int ContactId { get; set; }
}

public class GetContactEmailsQueryHandler : IRequestHandler<GetContactEmailsQuery, List<ContactEmailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContactEmailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ContactEmailDto>> Handle(GetContactEmailsQuery request, CancellationToken cancellationToken)
    {

        return await _context.ContactEmail
                .Where(ce => ce.ContactId == request.ContactId && !ce.IsDeleted)
                .ProjectToListAsync<ContactEmailDto>(_mapper.ConfigurationProvider);
    }
}