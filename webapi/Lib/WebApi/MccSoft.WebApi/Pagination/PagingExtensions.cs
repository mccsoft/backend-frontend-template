using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeinLinq;

namespace MccSoft.WebApi.Pagination
{
    public static class PagingExtensions
    {
        /// <summary>
        /// Create a paging list based on the EntityFramework query with a given sort order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Entity Framework query</param>
        /// <param name="pagedRequestDto">Dto with sorting and paging parameters</param>
        /// <param name="defaultSortExpression">Default sort expression</param>
        /// <param name="defaultLimit">Default limit to use if not specified in Dto.</param>
        /// /// <param name="allowedSortFields">fields for which sorting is allowed. If null - all fields will be allowed</param>
        public static Task<PagedResult<T>> ToPagingListAsync<T>(
            this IQueryable<T> query,
            PagedRequestDto pagedRequestDto,
            string defaultSortExpression,
            int defaultLimit = 20,
            IList<string>? allowedSortFields = null /* null means all fields are allowed */
        ) where T : class
        {
            return query.ToPagingListAsync(
                pagedRequestDto.Offset ?? 0,
                pagedRequestDto.Limit ?? defaultLimit,
                pagedRequestDto.SortBy,
                defaultSortExpression,
                pagedRequestDto.SortOrder,
                allowedSortFields: allowedSortFields
            );
        }

        /// <summary>
        /// Create a paging list based on the EntityFramework query with a given sort order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Entity Framework query</param>
        /// <param name="offset">The offset of the first record to return</param>
        /// <param name="limit">The number of records to return</param>
        /// <param name="sortExpression">Sort expression</param>
        /// <param name="defaultSortExpression">Default sort expression</param>
        /// <param name="sortOrder">direction of sorting</param>
        /// <param name="allowedSortFields">fields for which sorting is allowed. If null - all fields will be allowed</param>
        public static async Task<PagedResult<T>> ToPagingListAsync<T>(
            this IQueryable<T> query,
            int offset,
            int limit,
            string sortExpression,
            string defaultSortExpression,
            SortOrder sortOrder = SortOrder.Asc,
            IList<string>? allowedSortFields = null /* null means all fields are allowed */
        ) where T : class
        {
            var totalRecordCount = await query.CountAsync();

            var sort = string.IsNullOrEmpty(sortExpression)
              ? defaultSortExpression
              : sortExpression;

            if (sortOrder == SortOrder.Desc)
            {
                query = query.OrderBy(sort, @descending: true);
            }
            else
            {
                query = query.OrderBy(sort);
            }

            var data = await query.Skip(offset).Take(limit).ToListAsync().ConfigureAwait(false);
            return new PagedResult<T>(data, totalRecordCount);
        }
    }
}
