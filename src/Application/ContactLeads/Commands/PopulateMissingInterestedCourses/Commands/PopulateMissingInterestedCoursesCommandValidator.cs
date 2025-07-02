using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Commands;

[UsedImplicitly]
public sealed class PopulateMissingInterestedCoursesCommandValidator
    : AbstractValidator<PopulateMissingInterestedCoursesCommand>
{
    public PopulateMissingInterestedCoursesCommandValidator(IConfiguration configuration)
    {
        var apiKey = configuration["Constants:ApiKey"];

        RuleFor(r => r.ApiKey)
            .NotEmpty()
            .Equal(apiKey)
            .WithMessage("APIKEY not valid");
    }
}
