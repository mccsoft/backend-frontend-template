using System.Security.Claims;
using FluentAssertions;
using MccSoft.Testing;
using MccSoft.WebApi.SignedUrl;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.App.Tests;

public class SignUrlHelperTests : AppServiceTestBase
{
    public SignUrlHelperTests(ITestOutputHelper outputHelper) : base(outputHelper, null)
    {
        Sut = new SignUrlHelper(
            new OptionsWrapper<SignUrlOptions>(
                new SignUrlOptions() { Secret = "1234567890123456" }
            ),
            _userAccessorMock.Object
        );
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
