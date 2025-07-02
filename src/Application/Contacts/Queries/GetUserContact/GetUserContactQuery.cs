using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Contacts.Queries.GetUserContact;

public readonly record struct GetUserContactQuery(string? Email, string? Phone) : IRequest<List<ContactUserDto>>;

public class GetUserContactQueryHandler : IRequestHandler<GetUserContactQuery, List<ContactUserDto>>
{
    private readonly IApplicationDbContext _context;
    private const int ResponseLimited = 30;
    public GetUserContactQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ContactUserDto>> Handle(GetUserContactQuery request, CancellationToken cancellationToken)
    {
        var contactUsers = await _context.Processes
            .AsNoTracking()
            .Join(_context.Users, p => p.UserId, u => u.Id, (p, u) => new { Process = p, User = u })
            .Join(_context.ContactPhone.Where(cp => !cp.IsDeleted), pu => pu.Process.ContactId, cp => cp.ContactId,
                (pu, cp) => new { pu.User, pu.Process, cp.Phone })
            .Join(_context.ContactEmail.Where(cm => !cm.IsDeleted), pcp => pcp.Process.ContactId,
                cm => cm.ContactId, (pcp, cm) => new { pcp.User, pcp.Process, pcp.Phone, cm.Email })
            .Where(x =>  (!string.IsNullOrWhiteSpace(request.Phone) && x.Phone.Equals(request.Phone) )
                         || (!string.IsNullOrWhiteSpace(request.Email) && x.Email.Equals(request.Email)) )
            .Where(x => x.Process.Status != ProcessStatus.Closed)
            .OrderByDescending(x => x.User.Id)
            .Select(x => new ContactUserDto(
                    x.User.Id,
                    x.User.Name,
                    x.User.Surname,
                    x.User.Employee.CorporateEmail
                )
            )
            .Take(ResponseLimited)
            .ToListAsync(cancellationToken);

        return contactUsers;
    }
}