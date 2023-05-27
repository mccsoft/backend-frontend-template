using System.Security.Claims;
using MccSoft.WebApi.SignedUrl;

namespace MccSoft.TemplateApp.App.Tests;

public class SignUrlHelperTests : AppServiceTestBase
{
    public SignUrlHelperTests(ITestOutputHelper outputHelper) : base(outputHelper, null)
    {
        Sut = CreateService<SignUrlHelper>(x => x.AddSignUrl("fDmp1K2YveBbfDmpfDmp1K2YveBbfDmp"));
    }

    public SignUrlHelper Sut { get; set; }

    [Fact]
    public void GenerateSignedUrl()
    {
        _userAccessorMock.Setup(x => x.GetUserId()).Returns("5");

        var signature = Sut.GenerateSignature();
        var result = Sut.IsSignatureValid(signature, out var claims);
        result.Should().BeTrue();
        claims.FindFirstValue("id").Should().Be("5");
    }
}
