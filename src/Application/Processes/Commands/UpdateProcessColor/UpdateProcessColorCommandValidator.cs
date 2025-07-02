using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.UpdateProcessColor;

public class UpdateProcessColorCommandValidator : AbstractValidator<UpdateProcessColorCommand>
{
    private readonly IApplicationDbContext _context;
    
    public UpdateProcessColorCommandValidator(IApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(v => v.ProcessId)
            .NotEmpty().WithMessage("ProcessId is empty")
            .MustAsync(CheckProcessExists).WithMessage("ProcessId is wrong");

        RuleFor(v => v.Color)
            .NotEmpty().WithMessage("Color is empty");
    }
    
    private async Task<bool> CheckProcessExists( int processId, CancellationToken cancellationToken)
    {
        return await _context.Processes
            .AnyAsync(p => p.Id == processId && !p.IsDeleted, cancellationToken);
    }
}