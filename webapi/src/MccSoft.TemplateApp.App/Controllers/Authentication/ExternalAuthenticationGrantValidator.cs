using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MccSoft.TemplateApp.App.Controllers.Authentication
{
    /// <summary>
    /// Authenticates users via external OAuth providers.
    /// Uses special "external" GrantType (http://docs.identityserver.io/en/release/topics/extension_grants.html).
    /// </summary>
    public class ExternalAuthenticationUserProcessor<TUser, TKey>
        where TUser : IdentityUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly UserManager<TUser> _userManager;
        private readonly ILogger<ExternalAuthenticationUserProcessor<TUser, TKey>> _logger;

        public ExternalAuthenticationUserProcessor(
            UserManager<TUser> userManager,
            ILogger<ExternalAuthenticationUserProcessor<TUser, TKey>> logger
        )
        {
            _userManager = userManager;
            _logger = logger;
        }

        // public virtual async Task ValidateAsync(ExtensionGrantValidationContext context)
        // {
        //     Context = context;
        //     var oAuthCode = context.Request.Raw.Get("code");
        //     var providerName = context.Request.Raw.Get("provider");
        //     if (string.IsNullOrEmpty(oAuthCode) || string.IsNullOrEmpty(providerName))
        //     {
        //         context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
        //         return;
        //     }
        //
        //     var externalUserInfo = await _externalAuthService.GetExternalUserInfo(
        //         providerName,
        //         oAuthCode
        //     );
        //
        //     if (externalUserInfo == null)
        //     {
        //         context.Result = GetExternalUserNotFound();
        //         return;
        //     }
        //
        //     try
        //     {
        //         var user = await _userManager.FindByLoginAsync(providerName, externalUserInfo.Id);
        //         if (user != null)
        //         {
        //             context.Result = CreateValidationResultForUser(user);
        //         }
        //         else
        //         {
        //             context.Result = await CreateResultForLocallyNotFoundUser(
        //                 providerName,
        //                 externalUserInfo
        //             );
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         _logger.LogError(
        //             $"Exception while authorizing by external provider '{providerName}:{externalUserInfo.Id}', {e}"
        //         );
        //         context.Result = GetExternalUserNotFound();
        //     }
        // }

        /// <summary>
        /// Provides GrantValidationResult for the case when user was successfully authenticated by external OAuth provider, but no corresponding user was found locally
        /// (i.e. there's no corresponding entries in AspNetUserLogins table).
        /// You could override that method to auto-create local users.
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="externalUserId">User Id provided by external OAuth server</param>
        /// <returns>GrantValidationResult</returns>
        // protected virtual async Task<TUser> CreateResultForLocallyNotFoundUser(
        //     string providerName,
        //     ExternalLoginInfo externalUserInfo
        // )
        // {
        //     var user = await CreateNewUser(externalUserInfo);
        //     var createResult = await _userManager.CreateAsync(user);
        //     if (!createResult.Succeeded)
        //     {
        //         throw new Exception("User creation failed " + createResult);
        //     }
        //
        //     string displayName = GetDisplayName(externalUserInfo);
        //     return await AddLoginAndReturnResult(
        //         user,
        //         providerName,
        //         externalUserInfo.Id,
        //         displayName
        //     );
        // }

        protected virtual string GetDisplayName(ExternalLoginInfo externalLoginInfo)
        {
            return "";
        }

        /// <summary>
        /// If you have configured CreateUserIfNotFound to be true,
        /// this function will be called for every new user to be created.
        /// You could override it and provide your own implementation.
        /// </summary>
        /// <param name="externalLoginInfo">User information from OAuth provider</param>
        protected virtual Task<TUser> CreateNewUser(ExternalLoginInfo externalLoginInfo)
        {
            return Task.FromResult(new TUser() { UserName = GetUserName(externalLoginInfo), });
        }

        /// <summary>
        /// ASP.Net Identity requires UserName field to be filled.
        /// So you have to provide UserName for newly created users.
        /// By default it's externalUserInfo.Id.
        /// </summary>
        /// <param name="externalLoginInfo">User information from OAuth provider</param>
        protected virtual string GetUserName(ExternalLoginInfo externalLoginInfo) =>
            externalLoginInfo.LoginProvider + "_" + externalLoginInfo.ProviderKey;

        /// <summary>
        /// Returns the GrantValidationResult that should be returned to the Client
        /// when User was either unauthorized on OAuth provider,
        /// or we were unable to find it in local UserManager
        /// </summary>
        protected virtual ActionResult GetExternalUserNotFound()
        {
            return null;
        }
    }
}
