using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.UpdateContactLeadPrice;

public class UpdateContactLeadPriceCommandValidator : AbstractValidator<UpdateContactLeadPriceCommand>
{
    public UpdateContactLeadPriceCommandValidator()
    {
        RuleFor(cl => cl.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Discount must be greater than or equal to zero.");
        RuleFor(cl => cl.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Price must be greater than or equal to zero.");
        RuleFor(cl => cl.FinalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The FinalPrice must be greater than or equal to zero.");
        RuleFor(cl => cl.EnrollmentPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The EnrollmentPercentage must be greater than or equal to zero.");
        RuleFor(cl => cl.Fees)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Fees must be greater than or equal to zero.");
    }
}