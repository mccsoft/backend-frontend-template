using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MccSoft.TemplateApp.Http
{
    public class AuthenticationClient
    {
        private readonly HttpClient _client;

        private const string _authUrl = "connect/token";

        public AuthenticationClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> SignIn(string userName, string password)
        {
            var content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("userName", userName),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("scope", "profile")
                }
            );

            content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                "application/x-www-form-urlencoded"
            );
            var result = await _client.PostAsync(_authUrl, content);
            var stringContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new SignInException(stringContent);
            }

            return stringContent;
        }
    }
}
