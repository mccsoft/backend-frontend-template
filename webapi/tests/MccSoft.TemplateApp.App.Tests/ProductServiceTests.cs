using System.Threading.Tasks;
using FluentAssertions;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.TemplateApp.App.Features.Products;
using MccSoft.TemplateApp.App.Utils;
using MccSoft.TemplateApp.TestUtils.Factories;
using MccSoft.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MccSoft.TemplateApp.App.Tests
{
    public class ProductServiceTests : AppServiceTestBase<ProductService>
    {
        private ServiceProvider _serviceProvider;

        public ProductServiceTests() : base(DatabaseType.Postgres)
        {
            Sut = InitializeService();
        }

        [Fact]
        public async Task Create()
        {
            var result = await Sut.Create(a.CreateProductDto("asd"));
            result.Title.Should().Be("asd");
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
