using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetConctacById;

public class GetContactByIdQuery : IRequest<ContactFullDto>
{
    public int ContactId { get; set; }
}

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactFullDto>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetContactByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ContactFullDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .Include(c => c.ContactAddress.Where(i => !i.IsDeleted))
            .ThenInclude(a => a.Country)
            .ThenInclude(c => c.Currency)
            .Include(c => c.ContactAddress.Where(i => !i.IsDeleted))
            .ThenInclude(a => a.AddressType)
            .Include(c => c.ContactEmail.Where(i => !i.IsDeleted))
            .ThenInclude(e => e.EmailType)
            .Include(c => c.ContactPhone.Where(i => !i.IsDeleted))
            .ThenInclude(p => p.PhoneType)
            .Include(c => c.ContactLanguages.Where(i => !i.IsDeleted))
            .ThenInclude(l => l.Language)
            .Include(c => c.Processes)
            .Include(c => c.ContactTitles.Where(i => !i.IsDeleted))
            .ThenInclude(t => t.TitleType)
            .Include(c => c.Faculties.Where(i => !i.IsDeleted))
            .Include(c => c.Specialities.Where(i => !i.IsDeleted))
            .Include(c => c.Gender)
            .Include(s => s.Status)
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        if (contact == null)
        {
            throw new NotFoundException("Contact not found!");
        }
        return _mapper.Map<ContactFullDto>(contact);
    }
}