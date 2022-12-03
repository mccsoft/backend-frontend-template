using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MccSoft.PersistenceHelpers;

/// <summary>
/// Helper class for setting up global EF Query Filter, e.g. to only show entities that belong to the user.
/// </summary>
public static class DbContextQueryFilterExtensions
{
    /// <summary>
    /// Sets up a query filter. <typeparam name="T"></typeparam> is a base type for all Entities (e.g. TenantId or OrganisationId).
    /// Normally every Entity that should be filtered by TenantId/OrganisationId should have a base class/interface
    /// with TenantId property, that you could filter on.
    ///
    /// To integrate it add something like the following to OnModelCreating:
    /// builder.SetupQueryFilter<IOwnedEntity>(
    ///   (x) => CurrentOwnerId == null || x.OwnerId == CurrentOwnerId
    /// );
    /// </summary>
    public static void SetupQueryFilter<T>(
        this ModelBuilder builder,
        Expression<Func<T, bool>> filter,
        List<Type> typesToExclude = null
    )
    {
        typesToExclude ??= new List<Type>();

        var entityTypes = builder.Model
            .GetEntityTypes()
            .Where(
                entityType =>
                    typeof(T).IsAssignableFrom(entityType.ClrType)
                    && !typesToExclude.Contains(entityType.ClrType)
                    && entityType.BaseType == null
            )
            .ToList();

        entityTypes.ForEach(entityType =>
        {
            builder
                .Entity(entityType.ClrType)
                .HasQueryFilter(ConvertFilterExpression(filter, entityType.ClrType));
        });
    }

    private static LambdaExpression ConvertFilterExpression<TInterface>(
        Expression<Func<TInterface, bool>> filterExpression,
        Type entityType
    )
    {
        var newParam = Expression.Parameter(entityType);
        var newBody = ReplacingExpressionVisitor.Replace(
            filterExpression.Parameters.Single(),
            newParam,
            filterExpression.Body
        );

        return Expression.Lambda(newBody, newParam);
    }
}
