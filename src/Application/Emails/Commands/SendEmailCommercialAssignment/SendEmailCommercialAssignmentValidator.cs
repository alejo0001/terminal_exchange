
using CrmAPI.Application.Emails.Commands.SendEmailCommercialAssignment;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Emails.Commands.SendEmailCommercialAssignment;

[UsedImplicitly]
public sealed class SendEmailCommercialAssignmentValidator
    : AbstractValidator<SendEmailCommercialAssignmentCommand>
{
    public SendEmailCommercialAssignmentValidator(IConfiguration configuration)
    {
        var apiKey = configuration["Constants:ApiKey"];

        RuleFor(r => r.ApiKey)
            .NotEmpty()
            .Equal(apiKey)
            .WithMessage("APIKEY not valid");
    }
}