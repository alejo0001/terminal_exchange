using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.CheckContactIdCard;

public class CheckContactIdCardQuery : IRequest<ContactInfoDto>
{
    public required string IdCard { get; set; }
}

public class CheckContactIdCardQueryHandler : IRequestHandler<CheckContactIdCardQuery, ContactInfoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;    

    public CheckContactIdCardQueryHandler(IApplicationDbContext context, 
        IProcessesService processesService)
    {
        _context = context;
        _processesService = processesService;        
    }

    public async Task<ContactInfoDto> Handle(CheckContactIdCardQuery request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contact
            .FirstOrDefaultAsync(c => c.IdCard == request.IdCard && !c.IsDeleted, cancellationToken);

        if (contact == null)
        {
            return new ContactInfoDto()
            {
                Id = 0,
                ProcessId = 0,
                CanCreateProcess = true,
                IsBlackList = false
            };
        }

        var openProcess = await _context.Processes
            .Include(p => p.User)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.CurrentOrganizationNode)
            .FirstOrDefaultAsync(p => p.ContactId == contact.Id 
                                      && p.Status != ProcessStatus.Closed, cancellationToken);

        var canCreateProcess = openProcess == null;
        var replaceable = false;
        
        if (!canCreateProcess)
        {
            replaceable = await _processesService.CheckIfProcessIsReplaceable(openProcess!, cancellationToken);
        }
        
        return new ContactInfoDto()
        {
            Id = contact.Id,
            ProcessId = openProcess?.Id ?? 0,
            CanCreateProcess = canCreateProcess,
            IsBlackList = contact.ContactStatusId == (int) ContactStatusEnum.Blacklist,
            Replaceable = replaceable
        };
    }
}