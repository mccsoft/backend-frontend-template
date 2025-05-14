using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.PersistenceHelpers;
using MccSoft.TemplateApp.App.Features.Products.Dto;
using MccSoft.TemplateApp.App.Features.Webhooks;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.Pagination;
using MccSoft.WebApi.Patching;
using MccSoft.WebHooks.Publisher;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.TemplateApp.App.Features.Products;

[Log]
public class ProductService
{
    private readonly TemplateAppDbContext _dbContext;
    private readonly DbRetryHelper<TemplateAppDbContext, ProductService> _retryHelper;
    private readonly IUserAccessor _userAccessor;

    private readonly ILogger<ProductService> _logger;
    private readonly IWebHookEventPublisher _webHookEventPublisher;

    public ProductService(
        TemplateAppDbContext dbContext,
        DbRetryHelper<TemplateAppDbContext, ProductService> retryHelper,
        IUserAccessor userAccessor,
        ILogger<ProductService> logger,
        IWebHookEventPublisher webHookEventPublisher
    )
    {
        _dbContext = dbContext;
        _retryHelper = retryHelper;
        _userAccessor = userAccessor;
        _logger = logger;
        _webHookEventPublisher = webHookEventPublisher;
    }

    public async Task<ProductDto> Create(CreateProductDto dto)
    {
        // could be used if needed
        var userId = _userAccessor.GetUserId();

        var productId = await _retryHelper.RetryInTransactionAsync(
            async (db, logger) =>
            {
                var isProductWithSameNameExists = await db.Products.AnyAsync(
                    x => x.Title == dto.Title
                );
                if (isProductWithSameNameExists)
                {
                    throw new ValidationException(
                        nameof(dto.Title),
                        "Product with same name already exists"
                    );
                }

                var user = await db.Users.GetOne(User.HasId(userId));
                var product = new Product(dto.Title)
                {
                    ProductType = dto.ProductType,
                    LastStockUpdatedAt = dto.LastStockUpdatedAt,
                    CreatedByUser = user,
                };
                db.Products.Add(product);

                await db.SaveChangesAsync();
                logger.LogInformation("Product {id} created", product.Id);
                return product.Id;
            }
        );

        var result = await Get(productId);
        await _webHookEventPublisher.PublishEvent(WebHookEventType.NewProduct.ToString(), result);

        return result;
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

        await _webHookEventPublisher.PublishEvent(
            WebHookEventType.ProductDeleted.ToString(),
            new { Id = id }
        );
    }
}
