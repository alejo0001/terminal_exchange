using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

[UsedImplicitly]
public class AuthApiSettingsValidator : AbstractValidator<AuthApiSettings>
{
    public AuthApiSettingsValidator()
    {
        RuleFor(x => x.AuthUrl)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(AuthApiSettings)}.{{PropertyName}}` value");
    }
}
