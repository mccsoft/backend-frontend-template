using System.ComponentModel.DataAnnotations;
using MccSoft.TemplateApp.Domain;
using MccSoft.WebApi.Domain.Helpers;

namespace MccSoft.TemplateApp.App.Features.Products.Dto;

public class CreateProductDto
{
    [Required]
    [Trim]
    [MinLength(3)]
    public string Title { get; set; }

    public ProductType ProductType { get; set; }

    public DateOnly LastStockUpdatedAt { get; set; }
}
