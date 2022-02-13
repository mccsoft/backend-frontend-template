using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MccSoft.TemplateApp.Http;
using Newtonsoft.Json.Linq;

namespace MccSoft.TemplateApp.ComponentTests.Helpers
{
    public static class AuthHelper
    {
        public static async Task<JwtSecurityToken> SignInGetToken(
            this AuthenticationClient client,
            string userName,
            string password
        )
        {
            var content = await client.SignIn(userName, password);

            content.Should().NotBeNullOrEmpty();
            JObject json = JObject.Parse(content);
            var accessToken = json["access_token"].ToString();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(accessToken) as JwtSecurityToken;
            return jwtToken;
        }

        public static async Task<List<string>> SignInGetPermissions(
            this AuthenticationClient client,
            string userName,
            string password
        )
        {
            var token = await client.SignInGetToken(userName, password);
            var permissions = token.Claims.Select(claim => claim.Value).ToList();

            return permissions;
        }
    }
}
