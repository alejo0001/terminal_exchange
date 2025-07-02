using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

[UsedImplicitly]
public class HrApiSettingsValidator : AbstractValidator<HrApiSettings>
{
    public HrApiSettingsValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(HrApiSettings)}.{{PropertyName}}` value");
    }
}
