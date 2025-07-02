using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.ContactLeads.Commands.CreateMostProfitableInterestedCourses;

[UsedImplicitly]
public sealed class CreateMostProfitableInterestedCoursesCommandValidator
    : AbstractValidator<CreateMostProfitableInterestedCoursesCommand>
{
    // TODO: Please, replicate this to other validators created by this task.
    private const string EntityByIdNotFoundMessage = "{PropertyName} `{PropertyValue}` cannot be found.";

    public CreateMostProfitableInterestedCoursesCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Dto.ContactId)
            .NotEmpty()
            .MustAsync(IsExistingContactAsync)
            .WithMessage(EntityByIdNotFoundMessage);

        RuleFor(c => c.Dto.ProcessId)
            .NotEmpty()
            .MustAsync(IsExistingProcessAsync)
            .WithMessage(EntityByIdNotFoundMessage);

        RuleFor(c => c.Dto.TopCoursesCount)
            .NotEmpty();

        return;

        Task<bool> IsExistingContactAsync(int id, CancellationToken ct) =>
            CheckExistenceByIdAndIsDeleted(context.Contact, id, ct);

        Task<bool> IsExistingProcessAsync(int id, CancellationToken ct) =>
            CheckExistenceByIdAndIsDeleted(context.Processes, id, ct);
    }

    private static Task<bool> CheckExistenceByIdAndIsDeleted<TEntity>(
        IQueryable<TEntity> dbSet,
        int id,
        CancellationToken ct)
        where TEntity : BaseEntity =>
        dbSet.Where(c => c.Id == id)
            .Where(c => !c.IsDeleted)
            .AnyAsync(ct);
}
