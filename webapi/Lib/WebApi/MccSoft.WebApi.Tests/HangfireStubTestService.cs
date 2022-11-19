using System.Threading.Tasks;

public class HangfireStubTestService
{
    private readonly MyDbContext _dbContext;

    public HangfireStubTestService(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddEntity(string title)
    {
        _dbContext.Entity1.Add(new Entity1()
        {
            Title = title,
        });
        await _dbContext.SaveChangesAsync();
    }
}
