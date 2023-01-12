using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.Jobs;

namespace MccSoft.TemplateApp.App.Jobs;

public class ProductDataLoggerJob : JobBase
{
    private readonly ILogger<ProductDataLoggerJob> _logger;
    private readonly TemplateAppDbContext _db;

    public ProductDataLoggerJob(ILogger<ProductDataLoggerJob> logger, TemplateAppDbContext db)
        : base(logger)
    {
        _logger = logger;
        _db = db;
    }

    protected override async Task ExecuteCore()
    {
        var operationName = nameof(ProductDataLoggerJob);
        _logger.LogInformation($"Starting {operationName} scheduler job");

        var dbEntities = _db.Products.ToList();

        foreach (var dbEntity in dbEntities)
        {
            _logger.LogInformation($"Product in DB: {dbEntity.Title} ");
        }

        _logger.LogInformation($"Finished {operationName}");
    }
}
