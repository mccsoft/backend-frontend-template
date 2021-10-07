using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MccSoft.TemplateApp.Http
{
    public class IdentityServerClient
    {
        private readonly HttpClient _client;

        private const string _authUrl = "connect/token";

        private const string ClientId = "web-client";
        private const string ClientSecret = "a47f8b85-8629-49e1-975f-2f407ca451f9";

        private const string MobileClientId = "mobile-client";
        private const string MobileClientSecret = "DPdzNcy2vcV960RVITfTCEujmdXTehOPRqHOU4Q6fU4=";

        public IdentityServerClient(HttpClient client)
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}")
                )
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
