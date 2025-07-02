using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Queries.GetTypesActiveProcesses;

public class GetTypesActiveProcessesQuery : IRequest<List<ProcessTypeDto>>
{
}
    
public class GetTypesActiveProcessesQueryHandler : IRequestHandler<GetTypesActiveProcessesQuery, List<ProcessTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
        
    public GetTypesActiveProcessesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
        
    public async Task<List<ProcessTypeDto>> Handle(GetTypesActiveProcessesQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);

        List<ProcessType> processTypes = await _context.Processes
            .Where(p =>
                (p.UserId == user!.Id)
                && (p.Status == ProcessStatus.Ongoing || p.Status == ProcessStatus.ToDo)
                && (p.IsDeleted == false)
            )
            .GroupBy(p => p.Type)
            .Select(p => p.Key).ToListAsync(cancellationToken);
            

        List<ProcessTypeDto> list = new List<ProcessTypeDto>();
        foreach (var processType in processTypes)
        {
            list.Add(new ProcessTypeDto()
            {
                Label = Enum.GetName(processType).ToLower(),
                Value = Enum.GetName(processType)
            });
        }
            
        return list;
    }
}