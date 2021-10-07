using System.Security.Claims;
using System.Threading.Tasks;
using IdentityOAuthSpaExtensions.Services;
using IdentityServer4.Validation;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.PersistenceHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.App.Services.Authentication
{
    public class TemplateAppExternalAuthenticationGrantValidator
        : ExternalAuthenticationGrantValidator<User, string>
    {
        private readonly TemplateAppDbContext _dbContext;

        public TemplateAppExternalAuthenticationGrantValidator(
            ExternalAuthService externalAuthService,
            UserManager<User> userManager,
            IOptionsMonitor<ExternalAuthOptions> options,
            TemplateAppDbContext dbContext,
            ILogger<ExternalAuthenticationGrantValidator<User, string>> logger
        ) : base(externalAuthService, userManager, options, logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GrantValidationResult> CreateResultForLocallyNotFoundUser(
            string providerName,
            ExternalUserInfo externalUserInfo
        ) {
            var principal = externalUserInfo.Ticket.Principal;
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                string email = emailClaim.Value;
                User existingUser = await _dbContext.Users.GetOneOrDefault(User.HasEmail(email));
                if (existingUser != null)
                {
                    return await AddLoginAndReturnResult(
                        existingUser,
                        providerName,
                        externalUserInfo.Id,
                        GetDisplayName(externalUserInfo)
                    );
                }
            }

            return await base.CreateResultForLocallyNotFoundUser(providerName, externalUserInfo);
        }

        protected override async Task<User> CreateNewUser(ExternalUserInfo externalUserInfo)
        {
            ClaimsPrincipal principal = externalUserInfo.Ticket.Principal;
            Claim? emailClaim = principal.FindFirst(ClaimTypes.Email);
            Claim? givenName = principal.FindFirst(ClaimTypes.GivenName);
            Claim? lastName = principal.FindFirst(ClaimTypes.Surname);

            User user;
            if (emailClaim != null)
            {
                string email = emailClaim.Value;
                user = new User(email)
                {
                    FirstName = givenName?.Value,
                    LastName = lastName?.Value,
                };
            }
            else
            {
                user = new User(GetUserName(externalUserInfo))
                {
                    FirstName = givenName?.Value,
                    LastName = lastName?.Value,
                    Email = "",
                };
            }

            return user;
        }

        protected override string GetDisplayName(ExternalUserInfo externalUserInfo)
        {
            return JsonConvert.SerializeObject(externalUserInfo?.Ticket?.Principal?.Claims);
        }
    }
}
