using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Processes.Commands.ReassignAllUserProcesses;

public class ReassignAllUserProcessesCommandValidator : AbstractValidator<ReassignAllUserProcessesCommand>
{
    private readonly IApplicationDbContext _context;
        
    public ReassignAllUserProcessesCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.FromUserId)
            .NotEmpty().WithMessage("FromUserId is empty")
            .MustAsync(CheckUserExists).WithMessage("The id provided in the parameter 'FromUserId' does not correspond to that of a user!")
            .MustAsync(CheckUserIsEmployee).WithMessage("The id provided in the 'FromUserId' parameter does not correspond to an employee's id!.");
        
        RuleFor(v => v.ToUserId)
            .NotEmpty().WithMessage("ToUserId is empty")
            .MustAsync(CheckUserExists).WithMessage("The id provided in the parameter 'ToUserId' does not correspond to that of a user!")
            .MustAsync(CheckUserIsEmployee).WithMessage("The id provided in the 'ToUserId' parameter does not correspond to an employee's id!.");
    }
        
    private async Task<bool> CheckUserExists(int userId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Users
            .Where(u => !u.IsDeleted)
            .Any(u => u.Id == userId), cancellationToken);
    }
        
    private async Task<bool> CheckUserIsEmployee(int userId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Users
            .Where(u => u.EmployeeId != null)
            .Any(u => u.Id == userId), cancellationToken);
    }
}