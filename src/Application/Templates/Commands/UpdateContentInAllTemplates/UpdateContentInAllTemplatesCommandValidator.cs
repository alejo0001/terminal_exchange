using CrmAPI.Application.Proposals.Commands.UpdateContentInAllTemplates;
using FluentValidation;

namespace CrmAPI.Application.Processes.Commands.UpdateContentInAllTemplates;

public class UpdateContentInAllTemplatesCommandValidator : AbstractValidator<UpdateContentInAllTemplatesCommand>
{
    public UpdateContentInAllTemplatesCommandValidator()
    {
        RuleFor(v => v.ActualContent)
            .NotEmpty().WithMessage("ActualContent must be provided.");
    }
}