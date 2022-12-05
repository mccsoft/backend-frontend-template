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

    [Log]
    protected override async Task ExecuteCore()
    {
        var dbEntities = _db.Products.ToList();

        foreach (var dbEntity in dbEntities)
        {
            _logger.LogInformation($"Product in DB: {dbEntity.Title} ");
        }
    }
}
