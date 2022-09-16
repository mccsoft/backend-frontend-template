using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace MccSoft.WebApi.Localization;

/// <summary>
/// Determines the culture information for a request via the value of a cookie.
/// Uses the same cookie as i18next
/// </summary>
public class CustomCookieRequestCultureProvider : RequestCultureProvider
{
    /// <summary>
    /// Represent the default cookie name used to track the user's preferred culture information, which is ".AspNetCore.Culture".
    /// </summary>
    public static readonly string DefaultCookieName = "i18next";

    /// <summary>
    /// The name of the cookie that contains the user's preferred culture information.
    /// Defaults to <see cref="DefaultCookieName"/>.
    /// </summary>
    public string CookieName { get; set; } = DefaultCookieName;

    /// <inheritdoc />
    public override Task<ProviderCultureResult> DetermineProviderCultureResult(
        HttpContext httpContext
    )
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var cookie = httpContext.Request.Cookies[CookieName];

        if (string.IsNullOrEmpty(cookie))
        {
            return NullProviderCultureResult;
        }

        var providerResultCulture = ParseCookieValue(cookie);

        return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
    }

    /// <summary>
    /// Parses a <see cref="RequestCulture"/> from the specified cookie value.
    /// Returns <c>null</c> if parsing fails.
    /// </summary>
    /// <param name="value">The cookie value to parse.</param>
    /// <returns>The <see cref="RequestCulture"/> or <c>null</c> if parsing fails.</returns>
    public static ProviderCultureResult? ParseCookieValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new ProviderCultureResult(value, value);
    }
}
