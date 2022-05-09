using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MccSoft.IntegreSqlClient.Dto;
using Microsoft.Extensions.Logging;

namespace MccSoft.IntegreSqlClient;

public class IntegreSqlClient
{
    private readonly ILogger<IntegreSqlClient> _logger;
    private static HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Initializes IntegreSQL Client
    /// </summary>
    /// <param name="integreUri">
    /// IntegreURI, e.g. 'http://localhost:5000/api/v1/'.
    /// 'api/v1' should be included in the URI!
    /// </param>
    /// <param name="logger">Logger</param>
    public IntegreSqlClient(Uri integreUri, ILogger<IntegreSqlClient> logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = integreUri, };
    }

    /// <summary>
    /// Returns NULL if template database is already created/is being created.
    /// </summary>
    public async Task<CreateTemplateDto> InitializeTemplate(string hash)
    {
        var response = await _httpClient
            .PostAsJsonAsync("templates", new { hash = hash })
            .ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Locked)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateTemplateDto>().ConfigureAwait(false);
    }

    public async Task FinalizeTemplate(string hash)
    {
        var response = await _httpClient.PutAsync($"templates/{hash}", null).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task DiscardTemplate(string hash)
    {
        var response = await _httpClient.DeleteAsync($"templates/{hash}").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<GetDatabaseDto> GetTestDatabase(string hash)
    {
        var response = await _httpClient
            .GetFromJsonAsync<GetDatabaseDto>($"templates/{hash}/tests")
            .ConfigureAwait(false);
        return response;
    }

    public async Task ReturnTestDatabase(string hash, int id)
    {
        var response = await _httpClient
            .DeleteAsync($"templates/{hash}/tests/{id}")
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
