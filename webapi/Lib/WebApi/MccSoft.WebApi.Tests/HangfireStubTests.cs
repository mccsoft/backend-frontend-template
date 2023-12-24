using System.IO;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

public class HangfireStubTests : WebApiTestBase
{
    [Fact]
    public void HangfireStub_StaticMethod_Works()
    {
        var backgroundJobClient = CreateService<IBackgroundJobClient>();
        var file = "1.txt";
        File.Delete(file);
        backgroundJobClient.Enqueue(() => File.WriteAllText(file, "123"));

        File.Exists(file).Should().BeTrue();
        File.ReadAllText(file).Should().Be("123");
    }

    [Fact]
    public async Task HangfireStub_InstanceMethod_Works()
    {
        var backgroundJobClient = CreateService<IBackgroundJobClient>();
        backgroundJobClient.Enqueue<HangfireStubTestService>(x => x.AddEntity("zxc"));

        await WithDbContext(async db =>
        {
            (await db.Entity1.CountAsync()).Should().Be(1);
            var product = await db.Entity1.SingleAsync();
            product.Title.Should().Be("zxc");
        });
    }

    public HangfireStubTests(ITestOutputHelper outputHelper)
        : base(outputHelper, DatabaseType.Postgres) { }
}
