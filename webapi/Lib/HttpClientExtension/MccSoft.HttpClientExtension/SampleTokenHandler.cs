using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace MccSoft.HttpClientExtension;

/// <summary>
/// This is just a sample to implement ITokenHandler.
/// </summary>
public class SampleTokenHandler : ITokenHandler
{
    private readonly string _authServerUrl;
    private string? _accessToken = null;

    public SampleTokenHandler(string authServerUrl)
    {
        _authServerUrl = authServerUrl;
    }

    public async Task<string> GetAccessToken()
    {
        return _accessToken;
    }

    public async Task<string> RefreshToken()
    {
        var client = new HttpClient();
        var discovery = await client.GetDiscoveryDocumentAsync(_authServerUrl);

        TokenResponse? newToken = await client.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest()
            {
                Address = discovery.TokenEndpoint,
                ClientId = "clientid",
                ClientSecret = "secret",
            }
        );
        // TokenResponse? newToken = await client.RequestRefreshTokenAsync(
        //     new RefreshTokenRequest()
        //     {
        //         Address = discovery.TokenEndpoint,
        //         ClientId = "ClientId",
        //         RefreshToken = "",
        //         Scope = "",
        //     }
        // );

        if (newToken.IsError)
        {
            throw new Exception(
                $"Refresh token operation failed, server responded with code '{newToken.HttpStatusCode}', error: '{newToken.Error}', description: '{newToken.ErrorDescription}'"
            );
        }
        _accessToken = newToken.AccessToken;

        return _accessToken;
    }
}
