using System.ComponentModel.DataAnnotations;

namespace MccSoft.TemplateApp.App.Features.Products.Dto
{
    public class CreateProductDto
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }
    }
}
