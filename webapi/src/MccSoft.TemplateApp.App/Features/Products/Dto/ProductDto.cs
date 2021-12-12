using System;
using MccSoft.TemplateApp.Domain;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Features.Products.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ProductType ProductType { get; set; }

        public DateOnly LastStockUpdatedAt { get; set; }
    }
}
