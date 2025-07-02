using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Emails.Commands.SendEmailScholarshipForActivationWith2Courses;

[UsedImplicitly]
public sealed class SendEmailScholarshipForActivationWith2CoursesCommandValidator
    : AbstractValidator<SendEmailScholarshipForActivationWith2CoursesCommand>
{
    private const string EntityByIdNotFoundMessage = "{PropertyName} `{PropertyValue}` cannot be found.";

    public SendEmailScholarshipForActivationWith2CoursesCommandValidator(IApplicationDbContext context)
    {
        // TODO: use IsDeleted check in other similar validators of this dev task.
        RuleFor(c => c.Dto.ContactId)
            .GreaterThan(0)
            .MustAsync(IsExistingContactAsync)
            .WithMessage(EntityByIdNotFoundMessage);

        RuleFor(c => c.Dto.ContactLeadIds)
            .NotEmpty()
            .Must(ids => ids.Length == 2)
            .WithMessage(
                (_, ids) => $"Exactly 2 ContactLead Ids are required for this business use case, but found {
                    ids.Length}.")
            .ForEach(id => id.GreaterThan(0));

        return;

        Task<bool> IsExistingContactAsync(int id, CancellationToken ct) =>
            context.Contact
                .Where(c => c.Id == id)
                .Where(c => !c.IsDeleted)
                .AnyAsync(ct);
    }
}
