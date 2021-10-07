using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MccSoft.TemplateApp.Http.Generated;
using MccSoft.TemplateApp.TestUtils.Factories;
using Xunit;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class ProductTests : TestBase
    {
        private readonly ProductClient _productClient;

        public ProductTests()
        {
            _productClient = new ProductClient(Client);
        }

        [Fact]
        public async Task Create_Ok()
        {
            var title = "123";
            var createdProduct = await _productClient.CreateAsync(
                a.CreateProductGeneratedDto(title)
            );

            createdProduct.Title.Should().Be(title);
        }

        [Fact]
        public async Task Get_Ok()
        {
            var title = "123";
            var createdProduct = await _productClient.CreateAsync(
                a.CreateProductGeneratedDto(title)
            );

            var product = await _productClient.GetAsync(createdProduct.Id);
            product.Title.Should().Be(title);
        }

        [Fact]
        public async Task Search_Ok()
        {
            var title = "123";
            var createdProduct = await _productClient.CreateAsync(
                a.CreateProductGeneratedDto(title)
            );

            var results = await _productClient.SearchAsync("", null, null, null, null);
            results.TotalCount.Should().Be(1);
            results.Data.Select(x => x.Title).Should().BeEquivalentTo(new[] { title });
        }

        [Fact]
        public async Task Patch()
        {
            var title = "123";
            var createdProduct = await _productClient.CreateAsync(
                a.CreateProductGeneratedDto(title)
            );

            await _productClient.PatchAsync(
                createdProduct.Id,
                new PatchProductDto() { Title = "321" }
            );

            var result = await _productClient.GetAsync(createdProduct.Id);
            result.Title.Should().BeEquivalentTo("321");
        }

        [Fact]
        public async Task Delete_Ok()
        {
            var title = "123";
            var createdProduct = await _productClient.CreateAsync(
                a.CreateProductGeneratedDto(title)
            );

            await _productClient.DeleteAsync(createdProduct.Id);

            WithDbContext(
                db =>
                {
                    db.Products.Should().HaveCount(0);
                }
            );
        }
    }
}
