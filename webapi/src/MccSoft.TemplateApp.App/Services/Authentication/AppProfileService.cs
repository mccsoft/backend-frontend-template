using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MccSoft.TemplateApp.App.Services.Authentication
{
    public class AppProfileService : ProfileService<User>
    {
        private readonly TemplateAppDbContext _dbContext;
        private readonly IUserAccessor _userAccessor;

        public AppProfileService(
            TemplateAppDbContext dbContext,
            IUserAccessor userAccessor,
            UserManager<User> userManager,
            IUserClaimsPrincipalFactory<User> claimsFactory,
            ILogger<ProfileService<User>> logger
        ) : base(userManager, claimsFactory, logger)
        {
            _dbContext = dbContext;
            _userAccessor = userAccessor;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var userId = context.Subject?.GetSubjectId();
            var user = _dbContext.Users.First(x => x.Id == userId);

            // error if user is null is already logged in base.GetProfileDataAsync(context)
            if (user != null)
            {
                context.IssuedClaims.Add(
                    new Claim(JwtClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                ); // Name as single field, smth like `FirstName LastName`
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.NickName, user.UserName)); // userName
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Id, userId));
                //context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, "admin"));
            }
        }
    }
}
