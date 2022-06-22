using System.Threading.Tasks;
using FluentAssertions;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.TemplateApp.App.Features.Products;
using MccSoft.TemplateApp.TestUtils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MccSoft.TemplateApp.App.Tests
{
    public class ProductServiceTests : AppServiceTestBase<ProductService>
    {
        public ProductServiceTests() : base(DatabaseType.Postgres)
        {
            Sut = InitializeService();
        }

        [Fact]
        public async Task Create()
        {
            var result = await Sut.Create(a.CreateProductDto("asd"));
            result.Title.Should().Be("asd");

            await WithDbContext(async db =>
            {
                var product = await db.Products.SingleAsync();
                product.CreatedByUserId.Should().Be(_defaultUser.Id);
            });
        }

        [Fact]
        public async Task Get()
        {
            var createResult = await Sut.Create(a.CreateProductDto("asd"));
            var getResult = await Sut.Get(createResult.Id);
            getResult.Title.Should().Be("asd");
        }
    }
}
