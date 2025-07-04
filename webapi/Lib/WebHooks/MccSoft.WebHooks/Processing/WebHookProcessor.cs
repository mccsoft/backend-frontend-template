using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MccSoft.WebHooks.Configuration;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using MccSoft.WebHooks.Publisher;
using MccSoft.WebHooks.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;

namespace MccSoft.WebHooks.Processing;

public class WebHookProcessor<TSub>
    where TSub : WebHookSubscription
{
    private readonly DbContext _dbContext;
    private readonly ILogger<WebHookEventPublisher<TSub>> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly IWebHookInterceptors<TSub> _webHookInterceptors;
    private readonly IWebHookOptionBuilder<TSub> _configuration;
    private readonly IWebHookSignatureService<TSub> _webHookSignatureService;

    public WebHookProcessor(
        IServiceProvider serviceProvider,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<WebHookEventPublisher<TSub>> logger,
        IWebHookInterceptors<TSub> webHookInterceptors,
        IWebHookOptionBuilder<TSub> configuration,
        IWebHookSignatureService<TSub> webHookSignatureService
    )
    {
        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );

        _dbContext = (DbContext)
            serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);

        _webHookInterceptors = webHookInterceptors;
        _pipelineProvider = pipelineProvider;
        _logger = logger;
        _pipelineProvider = pipelineProvider;
        _configuration = configuration;
        _webHookSignatureService = webHookSignatureService;
    }

    [CustomRetry]
    public async Task RunWebHookDeliveryJob(
        int webHookId,
        CancellationToken cancellationToken = default
    )
    {
        var webHook = await _dbContext
            .WebHooks<TSub>()
            .Include(x => x.Subscription)
            .FirstAsync(x => x.Id == webHookId, cancellationToken: cancellationToken);

        webHook.ResetAttempts();
        await _dbContext.SaveChangesAsync(cancellationToken);

        _webHookInterceptors.BeforeExecution?.Invoke(webHook);

        if (webHook.AttemptsPerformed <= _configuration.ResilienceOptions.MaxRetryAttempts)
        {
            await TryToProcessWithPolly(webHook, cancellationToken);
        }
        else
        {
            await ProcessWebHook(webHook, cancellationToken);
        }

        _webHookInterceptors.ExecutionSucceeded?.Invoke(webHook);
    }

    public async Task TryToProcessWithPolly(
        WebHook<TSub> webHook,
        CancellationToken cancellationToken = default
    )
    {
        var pipeline = _pipelineProvider.GetPipeline("default");
        await pipeline.ExecuteAsync(
            async (webHook, cancellationToken) =>
            {
                await ProcessWebHook(webHook, cancellationToken);
            },
            webHook,
            cancellationToken
        );
    }

    private async Task ProcessWebHook(
        WebHook<TSub> webHook,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Starting {Method}, sending to {TargetUrl}",
            nameof(ProcessWebHook),
            webHook.Subscription.Url
        );

        var message = new HttpRequestMessage()
        {
            Method = new HttpMethod(webHook.Subscription.Method),
            Content = webHook.Data == null ? null : new StringContent(webHook.Data),
            RequestUri = new Uri(webHook.Subscription.Url),
        };

        if (_configuration.UseSigning && webHook.Subscription.IsSignatureDefined())
        {
            SignHttpMessage(webHook, message);
        }

        foreach (var item in webHook.Subscription.Headers)
        {
            message.Headers.Add(item.Key, item.Value);
        }

        using var client = new HttpClient();

        webHook.BeforeAttempt(DateTime.UtcNow);
        var result = await client.SendAsync(message, cancellationToken);
        var content = await result.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            result.EnsureSuccessStatusCode();
            _logger.LogInformation(
                "Method {Method}, sending to {TargetUrl} succeeded",
                nameof(ProcessWebHook),
                webHook.Subscription.Url
            );
            webHook.MarkSuccessful(result.StatusCode, content);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                e,
                "Method {Method}, sending to {TargetUrl} failed",
                nameof(ProcessWebHook),
                webHook.Subscription.Url
            );

            webHook.MarkFailed(result.StatusCode, e.Message);
            throw new HttpRequestException(content, e, result.StatusCode);
        }
        finally
        {
            _dbContext.Update(webHook);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Adds signature header into request with sent webhook data
    /// </summary>
    /// <param name="webHook">Sent webhook event</param>
    /// <param name="message">Http request message that will be enriched with signature</param>
    private void SignHttpMessage(WebHook<TSub> webHook, HttpRequestMessage message)
    {
        var secret = _webHookSignatureService.DecryptSecret(webHook.Subscription.SignatureSecret);
        var signature = _webHookSignatureService.ComputeSignature(webHook.Data, secret);

        message.Headers.Add(_configuration.WebHookSignatureHeaderName, signature);
    }
}
