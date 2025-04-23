using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.WebHooks.Domain;
using MccSoft.WebHooks.Interceptors;
using MccSoft.WebHooks.Publisher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;

namespace MccSoft.WebHooks.Processing;

/// <summary>
/// Handles the execution of WebHook delivery jobs, including retry policies and error handling.
/// </summary>
/// <typeparam name="TSub">The type of WebHook subscription.</typeparam>
public class WebHookProcessor<TSub>
    where TSub : WebHookSubscription
{
    private readonly DbContext _dbContext;
    private readonly ILogger<WebHookEventPublisher<TSub>> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly IWebHookInterceptors<TSub> _webHookInterceptors;
    private readonly WebHookOptionBuilder<TSub> _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebHookProcessor{TSub}"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve the required DbContext and services.</param>
    /// <param name="pipelineProvider">Polly resilience pipeline provider.</param>
    /// <param name="logger">Logger instance for logging execution details.</param>
    /// <param name="webHookInterceptors">Optional interceptors to hook into WebHook execution lifecycle.</param>
    /// <param name="configuration">WebHook execution configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown if the DbContext type was not registered.</exception>
    public WebHookProcessor(
        IServiceProvider serviceProvider,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<WebHookEventPublisher<TSub>> logger,
        IWebHookInterceptors<TSub> webHookInterceptors,
        WebHookOptionBuilder<TSub> configuration
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
    }

    /// <summary>
    /// Executes the delivery job for the specified WebHook, applying retry policies and invoking interceptors.
    /// </summary>
    /// <param name="webHookId">The ID of the WebHook to process.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

        _webHookInterceptors.BeforeExecution?.Invoke(webHook.Id);

        if (webHook.AttemptsPerformed <= _configuration.ResilienceOptions.MaxRetryAttempts)
        {
            await TryToProcessWithPolly(webHook, cancellationToken);
        }
        else
        {
            await ProcessWebHook(webHook, cancellationToken);
        }

        _webHookInterceptors.ExecutionSucceeded?.Invoke(webHook.Id);
    }

    /// <summary>
    /// Attempts to process the WebHook using Polly resilience policies (retry, timeout, etc.).
    /// </summary>
    /// <param name="webHook">The WebHook instance to process.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Sends the actual HTTP request for the given WebHook, handling success/failure and updating its status.
    /// </summary>
    /// <param name="webHook">The WebHook to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
}
