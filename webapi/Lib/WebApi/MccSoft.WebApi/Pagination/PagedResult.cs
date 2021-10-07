using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MccSoft.WebApi.Pagination
{
    public class PagedResult<T>
    {
        public PagedResult(IEnumerable<T> data, int totalCount)
        {
            Data = data.ToList();
            TotalCount = totalCount;
        }

        [Required]
        public IList<T> Data { get; set; }

        public int TotalCount { get; set; }
    }
}
