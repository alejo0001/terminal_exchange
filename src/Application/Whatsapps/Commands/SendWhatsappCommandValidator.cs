using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Whatsapps.Commands;

public class SendWhatsappCommandValidator : AbstractValidator<SendWhatsappCommand>
{

    public SendWhatsappCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.ContactId)
            .GreaterThan(0)
            .WithMessage("Contact Id is required and must be greater than 0.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.");
    }
}
