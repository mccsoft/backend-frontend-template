using MccSoft.Logging;
using MccSoft.TemplateApp.App.Features.Products.Dto;
using MccSoft.WebApi.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace MccSoft.TemplateApp.App.Features.Products;

[Authorize]
[Route("api/products")]
public class ProductController
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("")]
    public async Task<ProductDto> Create(CreateProductDto dto)
    {
        return await _productService.Create(dto);
    }

    [HttpPatch("{id:int}")]
    public async Task<ProductDto> Patch(int id, [FromBody] PatchProductDto dto)
    {
        using var logContext = LogContext.PushProperty(Field.ProductId, id);

        return await _productService.Patch(id, dto);
    }

    [HttpDelete("")]
    public async Task Delete(int id)
    {
        using var logContext = LogContext.PushProperty(Field.ProductId, id);

        await _productService.Delete(id);
    }

    [HttpGet]
    public async Task<PagedResult<ProductListItemDto>> Search([FromQuery] SearchProductDto dto)
    {
        return await _productService.Search(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ProductDto> Get(int id)
    {
        using var logContext = LogContext.PushProperty(Field.ProductId, id);

        return await _productService.Get(id);
    }
}
