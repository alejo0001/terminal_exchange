using FluentValidation;

namespace CrmAPI.Infrastructure.Settings.Validation;

[UsedImplicitly]
public class TranslationApiSettingsValidator : AbstractValidator<TranslationApiSettings>
{
    public TranslationApiSettingsValidator()
    {
        RuleFor(x => x.TranslationsUrl)
            .NotEmpty()
            .WithMessage($"Missing `{nameof(TranslationApiSettings)}.{{PropertyName}}` value");
    }
}
