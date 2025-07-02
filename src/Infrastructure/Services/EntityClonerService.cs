using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Services;

/// <inheritdoc />
[UsedImplicitly]
public sealed class EntityClonerService<TContext> : IEntityClonerService<TContext>
{
    private readonly IUnitOfWork<TContext> _unitOfWork;
    public EntityClonerService(IUnitOfWork<TContext> unitOfWork) => _unitOfWork = unitOfWork;

    public TEntity DeepCopyAndBaseEntityReset<TEntity>(TEntity existingEntity)
        where TEntity : BaseEntity, new()
    {
        // Steps order is important to not add entity with the same ID into Tracker.

        // Explicit Deep Clone.
        var newEntity = new TEntity();

        var entityEntry = _unitOfWork.Entry(newEntity);
        entityEntry.CurrentValues.SetValues(existingEntity);

        // Reset.
        newEntity.Id = default;
        newEntity.IsDeleted = false;
        newEntity.LastModified = default;
        newEntity.LastModifiedBy = default;

        // Add new entity into Tracker.
        entityEntry.State = EntityState.Added;

        return newEntity;
    }
}
