using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Processes.Commands.ReturnBusinessContactsToDelivery;

[UsedImplicitly]
public class ReturnBusinessContactsToDeliveryCommandValidator : AbstractValidator<ReturnBusinessContactsToDeliveryCommands>
{
    public ReturnBusinessContactsToDeliveryCommandValidator(IConfiguration configuration)
    {
        var apiKey = configuration["Constants:ApiKey"];
        RuleFor(r => r.ApiKey)
            .NotEmpty()
            .Equal(apiKey)
            .WithMessage("APIKEY not valid");
    }
}