using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using User = IntranetMigrator.Domain.Entities.User;

namespace CrmAPI.Application.Processes.Queries.GetCanRecoverProcesses;

public class GetCanCreateRecoverProcessQuery: IRequest<bool>
{
        
}
    
    
public class GetCanCreateRecoverProcessHandler : IRequestHandler<GetCanCreateRecoverProcessQuery, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
        
    public GetCanCreateRecoverProcessHandler(IApplicationDbContext context,  ICurrentUserService currentUserService,
        IConfiguration configuration)
    {
        _context = context;
        _currentUserService = currentUserService;
        _configuration = configuration;
    }
        

    public async Task<bool> Handle(GetCanCreateRecoverProcessQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.CurrentOrganizationNode)
                    .ThenInclude(o => o.Country)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
            
        if (user.Employee.CurrentOrganizationNode == null)
        {
            throw new NotFoundException(nameof(user.Employee.CurrentOrganizationNode));
        }


        var contactProcesses = await _context.Processes
            .Where(p => p.UserId == user.Id && p.ProcessOrigin == ProcessOrigin.Recover
                                            && p.Outcome == ProcessOutcome.NotSale)
            .ToListAsync(cancellationToken);

        // TODO: igual hay que crear un campo límite por usuario, por si hay comerciales que tienen más intentos que otros
        return contactProcesses.Count < (Int32.Parse(_configuration["Constants:LimitRecoverProcess"])); 

    }
}