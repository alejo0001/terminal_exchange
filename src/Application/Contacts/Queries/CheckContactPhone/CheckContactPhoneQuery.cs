using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.CheckContactPhone;

public class CheckContactPhoneQuery : IRequest<ContactInfoDto>
{
    public required string PhonePrefix { get; set; }
    public required string Phone { get; set; }
}
    
public class CheckContactEmailQueryHandler : IRequestHandler<CheckContactPhoneQuery, ContactInfoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;    

    public CheckContactEmailQueryHandler(IApplicationDbContext context,
        IProcessesService processesService )
    {
        _context = context;
        _processesService = processesService;        
    }
        
    public async Task<ContactInfoDto> Handle(CheckContactPhoneQuery request, CancellationToken cancellationToken)
    {
        var phone = await _context.ContactPhone
            .Include(ph => ph.Contact)
            .FirstOrDefaultAsync(ph => ph.PhonePrefix == request.PhonePrefix 
                                       && ph.Phone == request.Phone && !ph.IsDeleted, cancellationToken);

        if (phone == null)
        {
            return new ContactInfoDto()
            {
                Id = 0,
                CanCreateProcess = true,
                IsBlackList = false
            };
        }
            
        var openProcess = await _context.Processes
            .Include(p => p.User)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.CurrentOrganizationNode)
            .FirstOrDefaultAsync(p => p.ContactId == phone.Contact.Id 
                                      && p.Status != ProcessStatus.Closed, cancellationToken);

        var canCreateProcess = openProcess == null;
        var replaceable = false;
        
        if (!canCreateProcess)
        {
            replaceable = await _processesService.CheckIfProcessIsReplaceable(openProcess!, cancellationToken);
        }

        return new ContactInfoDto()
        {
            Id = phone.ContactId,
            ProcessId = openProcess?.Id ?? 0,
            CanCreateProcess = canCreateProcess,
            IsBlackList = phone.Contact.ContactStatusId == (int) ContactStatusEnum.Blacklist,
            Replaceable = replaceable
        };
    }
}