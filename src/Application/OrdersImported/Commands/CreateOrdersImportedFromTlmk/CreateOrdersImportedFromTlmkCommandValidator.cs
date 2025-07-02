using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.OrdersImported.Commands.CreateOrdersImportedFromTlmk;

public class CreateOrdersImportedFromTlmkCommandValidator : AbstractValidator<CreateOrdersImportedFromTlmkCommand>
{
    public CreateOrdersImportedFromTlmkCommandValidator(IConfiguration configuration)
    {
        RuleFor(r => r.ApiKey).NotEmpty()
            .Must(r => r.Equals(configuration["Constants:ApiKey"])).WithMessage("APIKEY not valid");
    }
}