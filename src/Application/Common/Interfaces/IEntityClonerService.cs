using IntranetMigrator.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Helps Unit Testing and should encapsulate some low-level ORM logic.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IEntityClonerService<in TContext>
{
    /// <summary>
    ///     Crates new instance of <see cref="TEntity" />, sets it property values from
    ///     <paramref name="existingEntity" />ts new entities
    ///     <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry{TEntity}" />'s
    ///     <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry{TEntity}.State" /> to
    ///     <see cref="EntityState.Added" />.<br />
    ///     Also resets properties of <see cref="BaseEntity" /> to their default values after copy.
    /// </summary>
    /// <remarks>
    ///     Guarantees that cloned entity is added to (EF) underlying ORM/Unit Of Work, so next call saving cahnges
    ///     will result in new in underlying databaase.
    /// </remarks>
    /// <param name="existingEntity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns>
    ///     New instance of <typeparamref name="TEntity" />.
    /// </returns>
    TEntity DeepCopyAndBaseEntityReset<TEntity>(TEntity existingEntity)
        where TEntity : BaseEntity, new();
}
