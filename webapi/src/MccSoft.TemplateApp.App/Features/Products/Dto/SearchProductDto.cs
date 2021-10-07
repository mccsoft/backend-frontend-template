using MccSoft.WebApi.Pagination;

namespace MccSoft.TemplateApp.App.Features.Products.Dto
{
    public class SearchProductDto : PagedRequestDto
    {
        public string? Search { get; set; }
    }
}
