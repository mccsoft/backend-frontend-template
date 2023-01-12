using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.ComponentTests.Infrastructure;

public class TestAuthenticationOptions : AuthenticationSchemeOptions
{
    public ClaimsIdentity Identity { get; set; } =
        new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "test"),
                new Claim("sub", "admin-should-be-replaced-by-user-id"),
            },
            "test"
        );

    public const string Scheme = "TestAuthenticationScheme";

    public TestAuthenticationOptions()
    {
        ClaimsIssuer = "test";
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authenticationTicket = new AuthenticationTicket(
            new ClaimsPrincipal(Options.Identity),
            new AuthenticationProperties(),
            TestAuthenticationOptions.Scheme
        );

        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }
}

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddTestAuthentication(
        this AuthenticationBuilder builder,
        Action<TestAuthenticationOptions> configureOptions
    )
    {
        return builder.AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
            TestAuthenticationOptions.Scheme,
            configureOptions
        );
    }
}
