using System;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Http.Generated;
using MccSoft.Testing.Infrastructure;
using ProductType = MccSoft.TemplateApp.Http.Generated.ProductType;

namespace MccSoft.TemplateApp.TestUtils.Factories;

public static class ProductFactory
{
    public static Product Product(
        this MotherFactory a,
        string title = "Default Product 1",
        string? userId = null,
        int tenantId = 1
    )
    {
        var product = new Product(title) { CreatedByUserId = userId };
        product.SetTenantIdUnsafe(tenantId);
        return product;
    }

    public static CreateProductDto CreateProductGeneratedDto(
        this MotherFactory a,
        string title = "Default Product 1",
        ProductType productType = ProductType.Undefined
    )
    {
        return new()
        {
            Title = title,
            ProductType = productType,
            LastStockUpdatedAt = new DateTimeOffset(new DateTime(2020, 1, 1))
        };
    }

    public static App.Features.Products.Dto.CreateProductDto CreateProductDto(
        this MotherFactory a,
        string title = "Default Product 1",
        Domain.ProductType productType = Domain.ProductType.Undefined
    )
    {
        return new() { Title = title, ProductType = productType };
    }
}
