using System.ComponentModel.DataAnnotations;
using MccSoft.TemplateApp.Domain;

namespace MccSoft.TemplateApp.App.Features.Products.Dto
{
    public class CreateProductDto
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }

        public ProductType ProductType { get; set; }
    }
}
