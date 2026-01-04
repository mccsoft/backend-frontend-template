using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MccSoft.DomainHelpers;
using MccSoft.DomainHelpers.IdInterfaces;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.PersistenceHelpers;

public static class QueryableExtensions
{
    /// <summary>
    /// Creates a "not found" exception based on the specified entity type and specification.
    /// </summary>
    private static Func<Type, string, string, Exception> _createException = DefaultExceptionFactory;
    private static Func<Type, string, string, Exception> _createForbiddenException =
        ForbiddenExceptionFactory;

    public static void UnsafeConfigureNotFoundException(
        Func<Type, string, string, Exception> exceptionFactory
    )
    {
        _createException = exceptionFactory;
    }

    /// <summary>
    /// Gets the single entity based on the specification.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <param name="messageIfNotFound">
    /// An additional message to add to the exception in case if the requested entity
    /// is not found.
    /// </param>
    /// <returns>The entity satisfying the specification.</returns>
    public static async Task<T> GetOne<T>(
        this IQueryable<T> repository,
        Specification<T> spec,
        string? messageIfNotFound = null
    )
        where T : class
    {
        T entity = await repository.GetOneOrDefault(spec);
        if (entity != null)
        {
            return entity;
        }

        // Todo: use named query filters (with EF 10)
        if (await repository.IgnoreQueryFilters().GetOneOrDefault(spec) != null)
            throw _createForbiddenException(typeof(T), spec.ToString(), messageIfNotFound);

        throw _createException(typeof(T), spec.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Determines whether exactly one entity matches the given specification.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <param name="messageIfNotFound">
    /// An additional message to add to the exception in case if the requested entity
    /// is not found.
    /// </param>
    /// <returns>Returns if the entity satisfying the specification exists.</returns>
    public static async Task HasOne<T>(
        this IQueryable<T> repository,
        Specification<T> spec,
        string messageIfNotFound = null
    )
        where T : class
    {
        T entity = await repository.GetOneOrDefault(spec);
        if (entity != null)
        {
            return;
        }

        // Todo: use named query filters (with EF 10)
        if (await repository.IgnoreQueryFilters().Where(spec).AnyAsync())
            throw _createForbiddenException(typeof(T), spec.ToString(), messageIfNotFound);

        throw _createException(typeof(T), spec.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity by id.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="id">id.</param>
    /// <param name="messageIfNotFound">
    /// An additional message to add to the exception in case if the requested entity
    /// is not found.
    /// </param>
    public static async Task<T> GetOneById<T>(
        this DbSet<T> repository,
        int id,
        string? messageIfNotFound = null
    )
        where T : class, IEntityWithIntKey
    {
        T entity = await repository.FindAsync(id);
        if (entity != null)
        {
            return entity;
        }

        throw _createException(typeof(T), id.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity by id.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="id">id.</param>
    /// <param name="messageIfNotFound">
    /// An additional message to add to the exception in case if the requested entity
    /// is not found.
    /// </param>
    public static async Task<T> GetOneById<T>(
        this DbSet<T> repository,
        Guid id,
        string? messageIfNotFound = null
    )
        where T : class, IEntityWithGuidKey
    {
        T entity = await repository.FindAsync(id);
        if (entity != null)
        {
            return entity;
        }

        throw _createException(typeof(T), id.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity by id.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="id">id.</param>
    /// <param name="messageIfNotFound">
    /// An additional message to add to the exception in case if the requested entity
    /// is not found.
    /// </param>
    public static async Task<T> GetOneById<T>(
        this IQueryable<T> repository,
        Guid id,
        string? messageIfNotFound = null
    )
        where T : class, IEntityWithGuidKey
    {
        T entity = await repository.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            return entity;
        }

        throw _createException(typeof(T), id.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity based on the id.
    /// Throws if the entity is not found.
    /// Throws if more than one entity has passed Id.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="id">The id.</param>
    /// <param name="select">Select statement.</param>
    /// <param name="messageIfNotFound">Message to add to exception if instance is not found</param>
    /// <returns>The entity with id.</returns>
    public static async Task<T2> GetOneById<T, T2>(
        this IQueryable<T> repository,
        int id,
        Expression<Func<T, T2>> select,
        string messageIfNotFound = null
    )
        where T : class, IEntityWithIntKey
    {
        T2 entity = await repository.Where(x => x.Id == id).Select(select).FirstOrDefaultAsync();
        if (entity != null)
        {
            return entity;
        }

        throw _createException(typeof(T), id.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity based on the id.
    /// Throws if the entity is not found.
    /// Throws if more than one entity has passed Id.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="id">The id.</param>
    /// <param name="select">Select statement.</param>
    /// <param name="messageIfNotFound">Message to add to exception if instance is not found</param>
    /// <returns>The entity with id.</returns>
    public static async Task<T2> GetOneById<T, T2>(
        this IQueryable<T> repository,
        Guid id,
        Expression<Func<T, T2>> select,
        string messageIfNotFound = null
    )
        where T : class, IEntityWithGuidKey
    {
        T2 entity = await repository.Where(x => x.Id == id).Select(select).FirstOrDefaultAsync();
        if (entity != null)
        {
            return entity;
        }

        throw _createException(typeof(T), id.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Gets the single entity based on the specification.
    /// Throws if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <param name="select">Select statement.</param>
    /// <param name="messageIfNotFound">Message to add to exception if instance is not found</param>
    /// <returns>The entity satisfying the specification.</returns>
    public static async Task<T2> GetOne<T, T2>(
        this IQueryable<T> repository,
        Specification<T> spec,
        Expression<Func<T, T2>> select,
        string messageIfNotFound = null
    )
        where T : class
    {
        T2 entity = await repository.Where(spec).Select(select).FirstOrDefaultAsync();
        if (entity != null)
        {
            return entity;
        }

        // Todo: use named query filters (with EF 10)
        if (await repository.IgnoreQueryFilters().Where(spec).Select(select).AnyAsync())
            throw _createForbiddenException(typeof(T), spec.ToString(), messageIfNotFound);

        throw _createException(typeof(T), spec.ToString(), messageIfNotFound);
    }

    /// <summary>
    /// Returns if an entity exists based on the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <returns>
    /// true in case if the entity exists otherwise false.
    /// </returns>
    public static Task<bool> Any<T>(this IQueryable<T> repository, Specification<T> spec)
        where T : class
    {
        return repository.AnyAsync(spec.Predicate);
    }

    /// <summary>
    /// Returns the number of elements in a sequence that satisfy the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <returns>
    /// Number of matching records.
    /// </returns>
    public static Task<int> Count<T>(this IQueryable<T> repository, Specification<T> spec)
        where T : class
    {
        return repository.CountAsync(spec.Predicate);
    }

    /// <summary>
    /// Filters the repository based on the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <returns>The repository filtered according to the specification.</returns>
    public static IQueryable<T> Where<T>(this IQueryable<T> repository, Specification<T> spec)
    {
        return repository.Where(spec.Predicate);
    }

    /// <summary>
    /// Gets the single entity based on the specification.
    /// Returns <c>null</c> if the entity is not found.
    /// Throws if more than one entity satisfies the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <returns>
    /// The entity satisfying the specification or <c>null</c> if no such entity exists.
    /// </returns>
    public static async Task<T> GetOneOrDefault<T>(
        this IQueryable<T> repository,
        Specification<T> spec
    )
        where T : class
    {
        return await repository.FirstOrDefaultAsync(spec.Predicate);
    }

    /// <summary>
    /// Gets a list of entities based on the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The entity repository.</param>
    /// <param name="spec">The specification.</param>
    /// <returns>The entities satisfying the specification.</returns>
    public static async Task<List<T>> Get<T>(this IQueryable<T> repository, Specification<T> spec)
        where T : class
    {
        return await repository.Where(spec.Predicate).ToListAsync();
    }

    private static Exception DefaultExceptionFactory(
        Type entityType,
        string specification,
        string messageIfNotFound
    )
    {
        messageIfNotFound = messageIfNotFound == null ? "" : messageIfNotFound + " ";
        throw new PersistenceResourceNotFoundException(
            $"{messageIfNotFound}The entity of type '{entityType.Name}' was not found. "
                + $"Specification: {specification}."
        );
    }

    private static Exception ForbiddenExceptionFactory(
        Type entityType,
        string specification,
        string message
    )
    {
        message = message == null ? "" : message + " ";
        throw new AccessDeniedException(
            $"{message}The entity of type '{entityType.Name}' can not be accessed. "
                + $"Specification: {specification}."
        );
    }
}
