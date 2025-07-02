using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using MediatR;

namespace CrmAPI.Application.Contacts.Queries.GetContactPhones;

public class GetContactPhonesQuery : IRequest<List<ContactPhoneDto>>
{
    public int ContactId { get; set; }
}

public class GetContactPhonesQueryHandler : IRequestHandler<GetContactPhonesQuery, List<ContactPhoneDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContactPhonesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ContactPhoneDto>> Handle(GetContactPhonesQuery request, CancellationToken cancellationToken)
    {

        return await _context.ContactPhone
                .Where(cp => cp.ContactId == request.ContactId && !cp.IsDeleted)
                .ProjectToListAsync<ContactPhoneDto>(_mapper.ConfigurationProvider);
    }
}