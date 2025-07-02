using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

public class CourseUnApiSettingsValidator : AbstractValidator<CourseUnApiSettings>
{
    public CourseUnApiSettingsValidator()
    {
        RuleFor(x => x.Enrollment)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(CourseUnApiSettings)}.{{PropertyName}}` value");
    }
}
