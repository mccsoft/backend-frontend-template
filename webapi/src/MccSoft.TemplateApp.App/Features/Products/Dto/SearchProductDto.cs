using MccSoft.TemplateApp.Domain;
using MccSoft.WebApi.Pagination;

namespace MccSoft.TemplateApp.App.Features.Products.Dto;

public class SearchProductDto : PagedRequestDto
{
    public string? Search { get; set; }
    public ProductType? ProductType { get; set; }

    public DateOnly? LastStockUpdatedAt { get; set; }

    public override string ToString()
    {
        return $"{nameof(Search)}: {Search}, {nameof(ProductType)}: {ProductType}, {nameof(LastStockUpdatedAt)}: {LastStockUpdatedAt}";
    }
}
