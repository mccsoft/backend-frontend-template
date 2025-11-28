using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MccSoft.HttpClientExtension;

/// <summary>
/// Handler to add the Bearer Token into a Http-Request and perform refresh token operation if needed.
/// Should be passed to HttpClient upon it's creation:
///   e.g. new HttpClient(new AuthenticationHandler(...))
///   or services.AddHttpClient<IMyApiService, MyApiService>().AddHttpMessageHandler<AuthenticationHandler>();
/// </summary>
public class AuthenticationHandler : DelegatingHandler
{
    private readonly ITokenHandler _tokenHandler;

    private readonly SemaphoreSlim _refreshTokenSemaphore = new SemaphoreSlim(1);

    public AuthenticationHandler(ITokenHandler tokenHandler, HttpClientHandler httpClientHandler)
        : base(httpClientHandler ?? new HttpClientHandler())
    {
        _tokenHandler = tokenHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _tokenHandler.GetAccessToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);

        if (
            response.StatusCode == HttpStatusCode.Unauthorized
            || response.StatusCode == HttpStatusCode.Forbidden
        )
        {
            // only one Handler should refresh token at a time
            await _refreshTokenSemaphore.WaitAsync(cancellationToken);
            try
            {
                // compare current token with initial one, and if it changed, reissue the request
                var newToken = await _tokenHandler.GetAccessToken();
                if (newToken != token)
                {
                    return await SendAsync(request, cancellationToken);
                }

                token = await _tokenHandler.RefreshToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                _refreshTokenSemaphore.Release();
            }
        }

        return response;
    }
}
