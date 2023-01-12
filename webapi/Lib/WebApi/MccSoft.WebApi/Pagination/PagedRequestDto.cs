namespace MccSoft.WebApi.Pagination;

/// <summary>
/// Paged Request DTO.
/// </summary>
public class PagedRequestDto
{
    /// <summary>
    /// Offset of list.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Number of requested records.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Field name for sorting in DB.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction. Ascending or Descending.
    /// </summary>
    public SortOrder SortOrder { get; set; }
}
