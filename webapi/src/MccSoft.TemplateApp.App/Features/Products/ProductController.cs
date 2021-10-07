using System.Threading.Tasks;
using MccSoft.TemplateApp.App.Features.Products.Dto;
using MccSoft.WebApi.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.TemplateApp.App.Features.Products
{
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
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public async Task<ProductDto> Create(CreateProductDto dto)
        {
            return await _productService.Create(dto);
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public async Task<ProductDto> Patch(int id, [FromBody] PatchProductDto dto)
        {
            return await _productService.Patch(id, dto);
        }

        [HttpDelete("")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public async Task Delete(int id)
        {
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
            return await _productService.Get(id);
        }
    }
}
