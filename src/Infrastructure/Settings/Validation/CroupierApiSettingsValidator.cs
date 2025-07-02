using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

[UsedImplicitly]
public class CroupierApiSettingsValidator : AbstractValidator<CroupierApiSettings>
{
    public CroupierApiSettingsValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(CroupierApiSettings)}.{{PropertyName}}` value");
    }
}
