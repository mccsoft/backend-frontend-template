using System.Linq;
using System.Threading.Tasks;
using MccSoft.NpgSql;
using MccSoft.TemplateApp.App.Features.Products.Dto;
using MccSoft.TemplateApp.App.Utils;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.PersistenceHelpers;
using MccSoft.WebApi.Pagination;
using MccSoft.WebApi.Patching;
using Microsoft.EntityFrameworkCore;
using NeinLinq;

namespace MccSoft.TemplateApp.App.Features.Products
{
    public class ProductService
    {
        private readonly TemplateAppDbContext _dbContext;
        private readonly PostgresRetryHelper<TemplateAppDbContext, ProductService> _retryHelper;

        public ProductService(
            TemplateAppDbContext dbContext,
            PostgresRetryHelper<TemplateAppDbContext, ProductService> retryHelper
        )
        {
            _dbContext = dbContext;
            _retryHelper = retryHelper;
        }

        public async Task<ProductDto> Create(CreateProductDto dto)
        {
            var productId = await _retryHelper.RetryInTransactionAsync(async db =>
            {
                var product = new Product(dto.Title)
                {
                    ProductType = dto.ProductType,
                    LastStockUpdatedAt = dto.LastStockUpdatedAt
                };
                db.Products.Add(product);

                await db.SaveChangesAsync();

                return product.Id;
            });
            return await Get(productId);
        }

        public async Task<ProductDto> Patch(int id, PatchProductDto dto)
        {
            Product product = await _dbContext.Products.GetOne(Product.HasId(id));
            product.Update(dto);

            await _dbContext.SaveChangesAsync();

            return await Get(product.Id);
        }

        public async Task<PagedResult<ProductListItemDto>> Search(SearchProductDto search)
        {
            IQueryable<Product> query = _dbContext.Products;

            if (!string.IsNullOrEmpty(search.Search))
            {
                query = query.Where(x => x.Title.Contains(search.Search));
            }

            if (search.ProductType != null)
            {
                query = query.Where(x => x.ProductType == search.ProductType);
            }

            return await query
                .Select(x => x.ToProductListItemDto())
                .ToPagingListAsync(search, nameof(ProductListItemDto.Id));
        }

        public async Task<ProductDto> Get(int id)
        {
            return await _dbContext.Products.GetOne(Product.HasId(id), x => x.ToProductDto());
        }

        public async Task Delete(int id)
        {
            var product = await _dbContext.Products.GetOne(Product.HasId(id));

            _dbContext.Products.Remove(product);

            await _dbContext.SaveChangesAsync();
        }
    }
}
