using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Contacts.Queries.GetContactInfoForTlmk;

public class GetContactInfoForTlmkQueryValidator : AbstractValidator<GetContactInfoForTlmkQuery>
{
    public GetContactInfoForTlmkQueryValidator(IConfiguration configuration)
    {
        RuleFor(r => r.ApiKey).NotEmpty()
            .Must(r => r.Equals(configuration["Constants:ApiKey"])).WithMessage("APIKEY not valid");
    }
}