using System.Security.Claims;
using FluentAssertions;
using MccSoft.Testing;
using MccSoft.WebApi.SignedUrl;
using Microsoft.Extensions.Options;
using Xunit;

namespace MccSoft.TemplateApp.App.Tests
{
    public class SignUrlHelperTests : AppServiceTestBase<SignUrlHelper>
    {
        public SignUrlHelperTests() : base(null)
        {
            Sut = InitializeService(
                (retryHelper, db) =>
                    new SignUrlHelper(
                        new OptionsWrapper<SignUrlOptions>(
                            new SignUrlOptions() { Secret = "1234567890123456" }
                        ),
                        _userAccessorMock.Object
                    )
            );
        }

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
}
