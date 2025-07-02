using FluentValidation;

namespace CrmAPI.Application.Contacts.Queries.GetUserContact;

public class GetUserContactQueryValidator : AbstractValidator<GetUserContactQuery>
{
    public GetUserContactQueryValidator()
    {
        RuleFor(x => x.Email)
            .Must((model, field) => !string.IsNullOrEmpty(model.Email) || !string.IsNullOrEmpty(model.Phone))
            .WithMessage("At least one of the fields must be different from null or empty.");
    }
}