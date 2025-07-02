using FluentValidation;

namespace CrmAPI.Application.Processes.Commands.ExternalSuccessfulSaleProcess;

public class ExternalSuccessfulSaleProcessCommandValidator : AbstractValidator<ExternalSuccessfulSaleProcessCommand>
{
    public ExternalSuccessfulSaleProcessCommandValidator()
    {
        RuleFor(v => v.ContactId)
            .NotEmpty().WithMessage("ContactId must be provided.");
        RuleFor(v => v.ProcessId)
            .NotEmpty().WithMessage("ProcessId must be provided.");
        RuleFor(v => v.OrderNumber)
            .NotEmpty().WithMessage("OrderNumber must be provided.");
    }
}