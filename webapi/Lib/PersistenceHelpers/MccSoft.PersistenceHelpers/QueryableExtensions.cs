using System;
using MccSoft.DomainHelpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives.Exceptions;

namespace MccSoft.PersistenceHelpers
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Creates a "not found" exception based on the specified entity type and specification.
        /// </summary>
        private static Func<Type, string, string, Exception> _createException =
            DefaultExceptionFactory;

        public static void UnsafeConfigureNotFoundException(
            Func<Type, string, string, Exception> exceptionFactory
        ) {
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
            string messageIfNotFound = null
        ) where T : class
        {
            T entity = await repository.GetOneOrDefault(spec);
            if (entity != null)
            {
                return entity;
            }

            throw _createException(typeof(T), spec.ToString(), messageIfNotFound);
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
        ) where T : class
        {
            T2 entity = await repository.Where(spec).Select(select).FirstOrDefaultAsync();
            if (entity != null)
            {
                return entity;
            }

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
        ) where T : class
        {
            return await repository.SingleOrDefaultAsync(spec.Predicate);
        }

        /// <summary>
        /// Gets a list of entities based on the specification.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="repository">The entity repository.</param>
        /// <param name="spec">The specification.</param>
        /// <returns>The entities satisfying the specification.</returns>
        public static async Task<List<T>> Get<T>(
            this IQueryable<T> repository,
            Specification<T> spec
        ) where T : class
        {
            return await repository.Where(spec.Predicate).ToListAsync();
        }

        private static Exception DefaultExceptionFactory(
            Type entityType,
            string specification,
            string messageIfNotFound
        ) {
            messageIfNotFound = messageIfNotFound == null ? "" : messageIfNotFound + " ";
            throw new PersistenceResourceNotFoundException(
                $"{messageIfNotFound}The entity of type '{entityType.Name}' was not found. "
                    + $"Specification: {specification}."
            );
        }
    }
}
