using FluentValidation;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Actions.Commands.UpdateAction;

public class UpdateActionCommandValidator : AbstractValidator<UpdateActionCommand>
{
    public UpdateActionCommandValidator()
    {
        RuleFor(v => v.ActionType)
            .IsEnumName(typeof(ActionType), false).WithMessage("Attribute ActionType is not a valid value.");
        RuleFor(v => v.Outcome)
            .IsEnumName(typeof(ActionOutcome), false).WithMessage("Attribute Outcome is not a valid value.");
    }
}