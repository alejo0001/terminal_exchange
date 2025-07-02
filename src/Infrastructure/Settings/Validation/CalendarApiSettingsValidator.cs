using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

public class CalendarApiSettingsValidator : AbstractValidator<CalendarApiSettings>
{
    public CalendarApiSettingsValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(CalendarApiSettings)}.{{PropertyName}}` value");
    }
}