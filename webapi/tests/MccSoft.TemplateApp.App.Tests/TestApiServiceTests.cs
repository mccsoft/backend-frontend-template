using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.TemplateApp.App.Features.TestApi;
using MccSoft.TemplateApp.TestUtils.Factories;

namespace MccSoft.TemplateApp.App.Tests;

public class TestApiServiceTests : AppServiceTestBase
{
    public TestApiServiceTests(ITestOutputHelper outputHelper)
        : base(outputHelper, DatabaseType.Postgres)
    {
        Sut = CreateService<TestApiService>();
    }

    public TestApiService Sut { get; set; }

    [Fact]
    public async Task CreateTestTenant_Simple()
    {
        await Sut.CreateTestTenant(
            new CreateTestTenantDto() { UserEmail = "a@a.ru", UserPassword = "123" }
        );
    }

    [Fact]
    public async Task ResetTenant_DoesntThrow()
    {
        await WithDbContext(async db =>
        {
            db.Products.Add(a.Product(userId: _defaultUser.Id));
            await db.SaveChangesAsync();
        });

        await Sut.ResetTenant();

        WithDbContextSync(db =>
        {
            db.Products.Count().Should().Be(0);
        });
    }
}
