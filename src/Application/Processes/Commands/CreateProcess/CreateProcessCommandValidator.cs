using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Processes.Commands.CreateProcess;

public class CreateProcessCommandValidator : AbstractValidator<CreateProcessCommand>
{
    private readonly IApplicationDbContext _context;
    public CreateProcessCommandValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(v => v.Type)
            .IsEnumName(typeof(ProcessType), false).WithMessage("Attribute Type is not a valid value.");
        RuleFor(v => v.Status)
            .IsEnumName(typeof(ProcessStatus), false).WithMessage("Attribute Status is not a valid value.");
        RuleFor(v => v.Outcome)
            .IsEnumName(typeof(ProcessOutcome), false).WithMessage("Attribute Outcome is not a valid value.");
        RuleFor(v => v.ContactId)
            .MustAsync(CheckContactHasProcessOpened).WithMessage("Contact has an opened process");
    }
        
    private async Task<bool> CheckContactHasProcessOpened(CreateProcessCommand command, int contactId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => !_context.Processes
            .Any(p => p.ContactId == contactId && p.Type == ProcessType.Sale
                                               && (p.Status == ProcessStatus.ToDo || p.Status == ProcessStatus.Ongoing)), cancellationToken);
    }
}