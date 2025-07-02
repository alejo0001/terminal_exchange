using System;
using System.Linq.Expressions;
using CrmAPI.Application.Common.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Services;

/// <inheritdoc />
/// <remarks>
///     This implementation uses directly <see cref="EF.Functions" /> for SQL generation from expression. <br />
///     It won't work in unit testing, because it is EF to datastore provider code.
/// </remarks>
public class EFCoreFunctions : IEFCoreFunctions
{
    public bool Like(string matchExpression, string pattern) => EF.Functions.Like(matchExpression, pattern);

    public Expression<Func<TEntity, bool>> LikeOr<TEntity>(
        Expression<Func<TEntity, string>> propertySelector,
        params string[] orPatterns)
    {
        var predicate = PredicateBuilder.New<TEntity>();
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        foreach (var pattern in orPatterns)
        {
            var methodInfo = typeof(DbFunctionsExtensions).GetMethod(
                nameof(DbFunctionsExtensions.Like),
                new[]
                {
                    typeof(DbFunctions), typeof(string), typeof(string),
                });

            var likeExpression = Expression.Call(
                methodInfo!,
                Expression.Constant(EF.Functions),
                property,
                Expression.Constant(pattern));

            predicate = predicate.Or(Expression.Lambda<Func<TEntity, bool>>(likeExpression, parameter));
        }

        return predicate;
    }
}
