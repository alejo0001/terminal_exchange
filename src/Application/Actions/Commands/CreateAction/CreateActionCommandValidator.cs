using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Commands.CreateAction;

public class CreateActionCommandValidator : AbstractValidator<CreateActionCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateActionCommandValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(v => v.ActionType)
            .IsEnumName(typeof(ActionType), false).WithMessage("Attribute ActionType is not a valid value.");
        RuleFor(v => v.Outcome)
            .IsEnumName(typeof(ActionOutcome), false).WithMessage("Attribute Outcome is not a valid value.");
        RuleFor(v => v.ProcessId)
            .MustAsync(CheckProcessExist).WithMessage("ProcessId do not exist");
    }

    private async Task<bool> CheckProcessExist(CreateActionCommand command, int processId,
        CancellationToken cancellationToken)
    {
        bool check = false;
        var process = await _context.Processes
            .Where(p => p.Id == command.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);
        if (process != null)
        {
            check = true;
        }

        return check;
    }

}