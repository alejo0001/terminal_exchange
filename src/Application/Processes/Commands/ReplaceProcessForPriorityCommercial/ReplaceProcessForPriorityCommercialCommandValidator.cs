using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Processes.Commands.ReplaceProcessForPriorityCommercial;

public class ReplaceProcessForPriorityCommercialCommandValidator : AbstractValidator<ReplaceProcessForPriorityCommercialCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationNodeExplorerService _organizationNodeExplorerService;

    public ReplaceProcessForPriorityCommercialCommandValidator(IApplicationDbContext context,
        IConfiguration configuration,
        ICurrentUserService currentUserService,
        IOrganizationNodeExplorerService organizationNodeExplorerService)
    {
        _context = context;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _organizationNodeExplorerService = organizationNodeExplorerService;

        RuleFor(v => v.ProcessId)
            .NotEmpty().WithMessage("ProcessId is required!")
            .MustAsync(CheckProcessExists).WithMessage("Process not found!.")
            .MustAsync(CheckProcessStatus).WithMessage("This action cannot be performed due to the current status of the process.")
            .MustAsync(CheckCurrentProcessOwnerIsReplaceable).WithMessage("Current permissions of the process owner prevented this action.");
            
        RuleFor(v => v)
            .MustAsync(CheckActualUserCanReplace).WithMessage("The user does not have permission to perform this action");
    }
    
    private async Task<bool> CheckProcessExists(int processId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Processes
            .Where(p => !p.IsDeleted)
            .Any(p => p.Id == processId), cancellationToken);
    }
    
    private async Task<bool> CheckProcessStatus(int processId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Processes
            .Any(p => p.Id == processId && p.Colour != Colour.Green), cancellationToken);
    }
    
    private async Task<bool> CheckActualUserCanReplace(ReplaceProcessForPriorityCommercialCommand forPriorityCommercialCommand, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.Employee.CorporateEmail == _currentUserService.Email, cancellationToken);
        
        var tagId = await _organizationNodeExplorerService
            .GetTagIdNode(user!.Employee.CurrentOrganizationNode, _configuration["Constants:TagCrmPriorityReplace"]!);

        return tagId > 0;
    }
    
    private async Task<bool> CheckCurrentProcessOwnerIsReplaceable(int processId, CancellationToken cancellationToken)
    {
        var employee = (await _context.Processes
            .Include(p => p.User)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.CurrentOrganizationNode)
            .FirstOrDefaultAsync(p => p.Id == processId, cancellationToken))!.User.Employee;
        
        var tagId = await _organizationNodeExplorerService
            .GetTagIdNode(employee.CurrentOrganizationNode, _configuration["Constants:TagCrmPriorityReplace"]!);

        return tagId == 0;
    }
}