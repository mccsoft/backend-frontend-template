/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using MccSoft.WebApi.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MccSoft.TemplateApp.App.Controllers.Authentication;

/// <summary>
/// Default implementation of AuthorizationController that allows logging in via external login providers
/// </summary>
public abstract class OpenIdAuthorizationControllerBase<TUser, TKey> : Controller
    where TUser : IdentityUser<TKey>, new()
    where TKey : IEquatable<TKey>
{
    protected readonly SignInManager<TUser> _signInManager;
    protected readonly UserManager<TUser> _userManager;
    private readonly IOpenIddictConfigurationProvider _configurationProvider;

    protected virtual string ControllerName => GetType().Name.Replace("Controller", "");

    protected OpenIdAuthorizationControllerBase(
        SignInManager<TUser> signInManager,
        UserManager<TUser> userManager,
        IOpenIddictConfigurationProvider configurationProvider
    )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configurationProvider = configurationProvider;
    }

    [AllowAnonymous]
    [HttpGet("~/connect/authorize/callback")]
    [HttpPost("~/connect/authorize/callback")]
    [IgnoreAntiforgeryToken]
    public virtual IActionResult ExternalCallback(string? remoteError, string originalQuery)
    {
        if (remoteError != null)
        {
            return BadRequest("Error from external provider. " + remoteError);
        }

        string redirectUrl = Url.Action(nameof(Authorize), ControllerName) + originalQuery;
        return LocalRedirect(redirectUrl!);
    }

    [AllowAnonymous]
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        ExternalLoginInfo externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo == null)
        {
            // If an identity provider was explicitly specified, redirect
            // the user agent to the AccountController.ExternalLogin action.
            var provider = (string)request["provider"];
            if (string.IsNullOrEmpty(provider))
            {
                return Content("No external authentication provider was specified");
            }

            var redirectUrl = Url.Action(
                nameof(ExternalCallback),
                ControllerName,
                new { originalQuery = HttpContext.Request.QueryString }
            );

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                redirectUrl
            );
            // Request a redirect to the external login provider.
            return Challenge(properties, provider);
        }

        try
        {
            TUser? user = await _userManager.FindByLoginAsync(
                externalLoginInfo.LoginProvider,
                externalLoginInfo.ProviderKey
            );

            if (user == null)
            {
                try
                {
                    user = await CreateUserFromExternalInfo(externalLoginInfo);
                }
                catch (Exception e)
                {
                    return Error(e.Message);
                }
            }

            return await SignInUser(user, request);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
    }

    protected virtual async Task<TUser> CreateUserFromExternalInfo(
        ExternalLoginInfo externalLoginInfo
    )
    {
        TUser user = await CreateNewUser(externalLoginInfo);

        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            throw new Exception(string.Join(";", identityResult.Errors));
        }

        await _userManager.AddLoginAsync(user, externalLoginInfo);

        return user;
    }

    /// <summary>
    /// If you have configured CreateUserIfNotFound to be true,
    /// this function will be called for every new user to be created.
    /// You could override it and provide your own implementation.
    /// </summary>
    /// <param name="externalUserInfo">User information from OAuth provider</param>
    protected virtual Task<TUser> CreateNewUser(ExternalLoginInfo externalUserInfo)
    {
        return Task.FromResult(new TUser() { UserName = GetUserName(externalUserInfo), });
    }

    /// <summary>
    /// ASP.Net Identity requires UserName field to be filled.
    /// So you have to provide UserName for newly created users.
    /// By default it's externalUserInfo.Id.
    /// </summary>
    /// <param name="externalUserInfo">User information from OAuth provider</param>
    protected virtual string GetUserName(ExternalLoginInfo externalUserInfo) =>
        externalUserInfo.LoginProvider + "_" + externalUserInfo.ProviderKey;

    [AllowAnonymous]
    [HttpPost("~/connect/token"), Produces("application/json")]
    public virtual async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request.IsAuthorizationCodeGrantType())
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
            if (!authenticateResult.Succeeded)
            {
                return StandardError();
            }

            IIdentity? identity = authenticateResult.Principal.Identity;
            string? userName = identity.Name;
            TUser user = await _userManager.FindByNameAsync(userName);

            return await SignInUser(user, request);
        }
        else if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return StandardError();
            }

            // Validate the username/password parameters and ensure the account is not locked out.
            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true
            );
            if (!result.Succeeded)
            {
                return StandardError();
            }

            return await SignInUser(user, request);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            var info = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );

            // Retrieve the user profile corresponding to the refresh token.
            // Note: if you want to automatically invalidate the refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                return Error("refresh_token_invalid");
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return StandardError();
            }

            return await SignInUser(user, request);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    protected virtual async Task<IActionResult> SignInUser(TUser user, OpenIddictRequest? request)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        if (
            !string.IsNullOrEmpty(request?.ClientId)
            && _configurationProvider.TryGetConfiguration(request.ClientId, out var configuration)
        )
        {
            if (configuration.RefreshTokenLifetime != null)
            {
                principal.SetRefreshTokenLifetime(
                    TimeSpan.FromSeconds(configuration.RefreshTokenLifetime.Value)
                );
            }

            if (configuration.AccessTokenLifetime != null)
            {
                principal.SetAccessTokenLifetime(
                    TimeSpan.FromSeconds(configuration.AccessTokenLifetime.Value)
                );
            }
        }

        await AddClaims(principal, user);

        var scopes = request.GetScopes();
        principal.SetScopes(scopes);

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    protected virtual IActionResult StandardError()
    {
        return Error("invalid_username_or_password");
    }

    protected virtual IActionResult Error(string description)
    {
        var properties = new AuthenticationProperties(
            new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }!
        );

        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    protected virtual async Task AddClaims(ClaimsPrincipal principal, TUser user)
    {
        IList<Claim> claims = await GetClaims(user);

        ClaimsIdentity claimIdentity = principal.Identities.First();
        claimIdentity.AddClaims(claims);
    }

    protected virtual async Task<IList<Claim>> GetClaims(TUser user)
    {
        return new List<Claim>()
        {
            new(JwtClaimTypes.NickName, user.UserName),
            new(JwtClaimTypes.Id, user.Id.ToString() ?? string.Empty),
            new(JwtClaimTypes.Subject, user.Id.ToString() ?? string.Empty),
        };
    }

    protected virtual IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
