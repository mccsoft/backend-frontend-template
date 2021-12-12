using System;
using System.Linq.Expressions;
using MccSoft.TemplateApp.App.Features.Products.Dto;
using MccSoft.TemplateApp.Domain;
using NeinLinq;

namespace MccSoft.TemplateApp.App.Features.Products
{
    public static class ProductExtensions
    {
        [InjectLambda]
        public static ProductDto ToProductDto(this Product product)
        {
            return ToProductDtoExpressionCompiled.Value(product);
        }

        private static readonly Lazy<Func<Product, ProductDto>> ToProductDtoExpressionCompiled =
            new(() => ToProductDto().Compile());

        public static Expression<Func<Product, ProductDto>> ToProductDto() =>
            product =>
                new ProductDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    ProductType = product.ProductType,
                    LastStockUpdatedAt = product.LastStockUpdatedAt,
                };

        [InjectLambda]
        public static ProductListItemDto ToProductListItemDto(this Product product)
        {
            return ToProductListItemDtoExpressionCompiled.Value(product);
        }

        private static readonly Lazy<
            Func<Product, ProductListItemDto>
        > ToProductListItemDtoExpressionCompiled = new(() => ToProductListItemDto().Compile());

        public static Expression<Func<Product, ProductListItemDto>> ToProductListItemDto() =>
            product =>
                new ProductListItemDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    ProductType = product.ProductType,
                    LastStockUpdatedAt = product.LastStockUpdatedAt,
                };
    }
}
