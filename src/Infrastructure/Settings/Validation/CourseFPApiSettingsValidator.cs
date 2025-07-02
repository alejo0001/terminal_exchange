using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

public class CourseFPApiSettingsValidator : AbstractValidator<CourseFPApiSettings>
{
    public CourseFPApiSettingsValidator()
    {
        RuleFor(x => x.Enrollment)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(CourseFPApiSettings)}.{{PropertyName}}` value");
    }
}
