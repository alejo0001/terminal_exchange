using CrmAPI.Application.Common.Interfaces;
using DeepCopy;
using IntranetMigrator.Domain.Common;

namespace Application.UnitTests;

public sealed class StubEntityClonerService<TContext> : IEntityClonerService<TContext>
{
    private readonly IUnitOfWork<TContext> _unitOfWork;
    public StubEntityClonerService(IUnitOfWork<TContext> unitOfWork) => _unitOfWork = unitOfWork;

    public TEntity DeepCopyAndBaseEntityReset<TEntity>(TEntity existingEntity)
        where TEntity : BaseEntity, new()
    {
        // "Steps order is important to not add entity with the same ID into ChangeTracker."

        // "Explicit Deep Clone."
        var newEntity = DeepCopier.Copy(existingEntity);

        // Reset.
        newEntity.Id = default;
        newEntity.IsDeleted = false;
        newEntity.LastModified = default;
        newEntity.LastModifiedBy = default;

        // Imitate: "Add new entity into ChangeTracker."
        _unitOfWork.Set<TEntity>().Add(newEntity);

        return newEntity;
    }
}
