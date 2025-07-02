using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.CheckContactEmail;

public class CheckContactEmailQuery : IRequest<ContactInfoDto>
{
    public required string Email { get; set; }
}
    
public class CheckContactEmailQueryHandler : IRequestHandler<CheckContactEmailQuery, ContactInfoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;    

    public CheckContactEmailQueryHandler(IApplicationDbContext context,
        IProcessesService processesService)
    {
        _context = context;
        _processesService = processesService;        
    }
        
    public async Task<ContactInfoDto> Handle(CheckContactEmailQuery request, CancellationToken cancellationToken)
    {
        var contactEmail = await _context.ContactEmail
            .Include(ce => ce.Contact)
            .FirstOrDefaultAsync(ce => ce.Email == request.Email && !ce.IsDeleted, cancellationToken);

        if (contactEmail == null)
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
            .FirstOrDefaultAsync(p => p.ContactId == contactEmail.ContactId 
                        && p.Status != ProcessStatus.Closed, cancellationToken);

        var canCreateProcess = openProcess == null;
        var replaceable = false;
        
        if (!canCreateProcess)
        {
            replaceable = await _processesService.CheckIfProcessIsReplaceable(openProcess!, cancellationToken);
        }

        return new ContactInfoDto()
        {
            Id = contactEmail.ContactId,
            ProcessId = openProcess?.Id ?? 0,
            CanCreateProcess = canCreateProcess,
            IsBlackList = contactEmail.Contact.ContactStatusId == (int) ContactStatusEnum.Blacklist,
            Replaceable = replaceable
        };
    }
}