using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Contacts.Commands.CreateContactLead;

public class CreateContactLeadCommandValidator : AbstractValidator<CreateContactLeadCommand>
{
        
    private readonly IApplicationDbContext _context;

    public CreateContactLeadCommandValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(cl => cl.ContactId)
            .MustAsync(CheckContactId).WithMessage("ContactId is not exist");
      /*  RuleFor(cl => cl.CourseId)
            .MustAsync(CheckCourseId).WithMessage("CourseId is not exist");*/
        RuleFor(cl => cl.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Discount must be greater than or equal to zero.");
        RuleFor(cl => cl.FinalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The FinalPrice must be greater than or equal to zero.");
        RuleFor(cl => cl.EnrollmentPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The EnrollmentPercentage must be greater than or equal to zero.");
        RuleFor(cl => cl.Fees)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Fees must be greater than or equal to zero.");
        RuleFor(cl => cl.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Price must be greater than or equal to zero.");
    }
        
        
    private async Task<bool> CheckContactId(CreateContactLeadCommand command, int contactId,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Contact.Any(c => c.Id == contactId), cancellationToken);
    }
        
   
}