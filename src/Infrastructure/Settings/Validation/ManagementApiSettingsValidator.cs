using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

public class ManagementApiSettingsValidator : AbstractValidator<ManagementApiSettings>
{
    public ManagementApiSettingsValidator()
    {
        RuleFor(x=> x.Enrollment)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(ManagementApiSettings)}.{{PropertyName}}` value");

    }
}