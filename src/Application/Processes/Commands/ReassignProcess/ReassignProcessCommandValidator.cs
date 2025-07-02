using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Processes.Commands.ReassignProcess;

public class ReassignProcessCommandValidator : AbstractValidator<ReassignProcessCommand>
{
    private readonly IApplicationDbContext _context;
        
    public ReassignProcessCommandValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(v => v.ProcessId)
            .MustAsync(CheckProcessExists).WithMessage("Process not found!.");
            
        RuleFor(v => v.UserId)
            .MustAsync(CheckUserExists).WithMessage("User not found!.");
            
        RuleFor(v => (v))
            .MustAsync(CheckNotReassignToTheSameUser).WithMessage("this process is already assigned to the proposed user.");
    }
        
    private async Task<bool> CheckProcessExists(ReassignProcessCommand command, int processId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Processes
            .Where(p => p.IsDeleted == false)
            .Any(p => p.Id == processId), cancellationToken);
    }
        
    private async Task<bool> CheckUserExists(ReassignProcessCommand command, int userId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Users
            .Where(u => u.IsDeleted == false)
            .Any(u => u.Id == userId), cancellationToken);
    }
    private async Task<bool> CheckNotReassignToTheSameUser(ReassignProcessCommand command, CancellationToken cancellationToken)
    {
        return await Task.Run(() => !_context.Processes
            .Where(p => p.Id == command.ProcessId)
            .Any(p => p.UserId == command.UserId), cancellationToken);
    }
}