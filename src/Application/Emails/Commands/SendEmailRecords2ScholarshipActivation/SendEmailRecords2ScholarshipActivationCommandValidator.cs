using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Emails.Commands.SendEmailRecords2ScholarshipActivation;

[UsedImplicitly]
public sealed class SendEmailRecords2ScholarshipActivationCommandValidator
    : AbstractValidator<SendEmailRecords2ScholarshipActivationCommand>
{
    public SendEmailRecords2ScholarshipActivationCommandValidator(IConfiguration configuration)
    {
        var apiKey = configuration["Constants:ApiKey"];

        RuleFor(r => r.ApiKey)
            .NotEmpty()
            .Equal(apiKey)
            .WithMessage("APIKEY not valid");
    }
}
