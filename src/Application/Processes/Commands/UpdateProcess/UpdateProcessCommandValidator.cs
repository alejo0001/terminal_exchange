using FluentValidation;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Processes.Commands.UpdateProcess;

public class UpdateProcessCommandValidator : AbstractValidator<UpdateProcessCommand>
{
    public UpdateProcessCommandValidator()
    {
        RuleFor(v => v.Type)
            .IsEnumName(typeof(ProcessType), false).WithMessage("Attribute Type is not a valid value.");
        RuleFor(v => v.Status)
            .IsEnumName(typeof(ProcessStatus), false).WithMessage("Attribute Status is not a valid value.");
        RuleFor(v => v.Outcome)
            .IsEnumName(typeof(ProcessOutcome), false).WithMessage("Attribute Outcome is not a valid value.");
    }
}