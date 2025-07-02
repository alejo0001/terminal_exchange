using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Initially created to help to unit test the code tha uses <see cref="EF.Functions" /> to provide abstraction over
///     a Stub and EF Core compatible implementations.
/// </summary>
public interface IEFCoreFunctions
{
    /// <inheritdoc cref="DbFunctionsExtensions.Like(Microsoft.EntityFrameworkCore.DbFunctions,string,string)" />
    bool Like(string matchExpression, string pattern);

    /// <summary>
    ///     Using <see cref="LinqKit" /> goodies to build more sophisticated expression that essential combines multiple
    ///     <see cref="DbFunctionsExtensions.Like(Microsoft.EntityFrameworkCore.DbFunctions,string,string)" /> into
    ///     generated SQL.
    /// </summary>
    /// <param name="propertySelector"></param>
    /// <param name="orPatterns"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Expression<Func<TEntity, bool>> LikeOr<TEntity>(
        Expression<Func<TEntity, string>> propertySelector,
        params string[] orPatterns);
}
