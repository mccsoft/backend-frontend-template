using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class WebHookProcessor : IWebHookInterceptor
{
    private readonly DbContext _dbContext;
    private readonly IEnumerable<IWebHookInterceptor> _interceptors;
    private readonly WebHookConfiguration _webHookConfiguration;
    private readonly ILogger<WebHookProcessor> _logger;
    private static readonly DateTime LastRunNextDateTime;

    static WebHookProcessor()
    {
        LastRunNextDateTime = DateTime.MaxValue;
        DateTime.SpecifyKind(LastRunNextDateTime, DateTimeKind.Utc);
    }

    public WebHookProcessor(
        IServiceProvider serviceProvider,
        IEnumerable<IWebHookInterceptor> interceptors,
        WebHookConfiguration webHookConfiguration,
        ILogger<WebHookProcessor> logger
    )
    {
        if (WebHookRegistration.DbContextType == null)
            throw new ArgumentNullException(
                $"WebHookRegistration.DbContextType wasn't initialized. Did you call builder.AddWebHookEntities() from DbContext OnModelCreating?"
            );

        _dbContext = (DbContext)
            serviceProvider.GetRequiredService(WebHookRegistration.DbContextType);
        _interceptors = interceptors;
        _webHookConfiguration = webHookConfiguration;
        _logger = logger;
    }

    public virtual async Task RunJob(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting {Method}", nameof(RunJob));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            stoppingToken
        );
        var webHooks = await _dbContext
            .WebHooks()
            .Where(x => !x.IsSucceeded && x.NextRun < DateTime.UtcNow)
            .Take(10)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken: stoppingToken);

        AdjustNextRun(_webHookConfiguration, webHooks);
        await _dbContext.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);

        foreach (var webHook in webHooks)
        {
            try
            {
                var result = await ProcessWebHook(webHook);
                webHook.MarkSuccessful(result);
                await ExecutionSucceeded(webHook);
            }
            catch (Exception e)
            {
                webHook.MarkFailed(e);
                await ExecutionFailed(webHook, e);

                if (webHook.NextRun == LastRunNextDateTime)
                {
                    _logger.LogInformation(
                        e,
                        "Method {Method}, sending to {TargetUrl} failed completely, no more retries will be performed",
                        nameof(RunJob),
                        webHook.TargetUrl
                    );

                    webHook.MarkFailedNoRetry();
                    await AfterAllAttemptsFailed(webHook);
                }
            }
            finally
            {
                await _dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }

    protected virtual async Task<string> ProcessWebHook(WebHook webHook)
    {
        _logger.LogInformation(
            "Starting {Method}, sending to {TargetUrl}",
            nameof(ProcessWebHook),
            webHook.TargetUrl
        );

        var client = new HttpClient();
        var message = new HttpRequestMessage()
        {
            Method = new HttpMethod(webHook.HttpMethod),
            Content =
                webHook.SerializedBody == null ? null : new StringContent(webHook.SerializedBody),
            RequestUri = new Uri(webHook.TargetUrl),
        };

        foreach (var item in webHook.Headers)
        {
            message.Headers.Add(item.Key, item.Value);
        }

        var result = await client.SendAsync(message);

        var content = await result.Content.ReadAsStringAsync();

        try
        {
            result.EnsureSuccessStatusCode();
            _logger.LogInformation(
                "Method {Method}, sending to {TargetUrl} succeeded",
                nameof(ProcessWebHook),
                webHook.TargetUrl
            );
            return content;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                e,
                "Method {Method}, sending to {TargetUrl} failed",
                nameof(ProcessWebHook),
                webHook.TargetUrl
            );
            throw new HttpRequestException(content, e, result.StatusCode);
        }
    }

    protected virtual void AdjustNextRun(WebHookConfiguration configuration, List<WebHook> webHooks)
    {
        var delaysConfiguration = configuration.SendingDelaysInMinutes;
        var now = DateTime.UtcNow;
        foreach (var webHook in webHooks)
        {
            if (delaysConfiguration.Length <= webHook.AttemptsPerformed)
            {
                webHook.BeforeRun(now, LastRunNextDateTime);
            }
            else
            {
                var delayTillNextExecution = delaysConfiguration[webHook.AttemptsPerformed];
                webHook.BeforeRun(now, now.AddMinutes(delayTillNextExecution));
            }
        }
    }

    public async Task BeforeExecutionAttempt(
        WebHook webHook,
        HttpClient httpClient,
        HttpRequestMessage message
    )
    {
        foreach (var interceptor in _interceptors)
        {
            await interceptor.BeforeExecutionAttempt(webHook, httpClient, message);
        }
    }

    public async Task ExecutionSucceeded(WebHook webHook)
    {
        foreach (var interceptor in _interceptors)
        {
            await interceptor.ExecutionSucceeded(webHook);
        }
    }

    public async Task ExecutionFailed(WebHook webHook, Exception e)
    {
        foreach (var interceptor in _interceptors)
        {
            await interceptor.ExecutionFailed(webHook, e);
        }
    }

    public async Task AfterAllAttemptsFailed(WebHook webHook)
    {
        foreach (var interceptor in _interceptors)
        {
            await interceptor.AfterAllAttemptsFailed(webHook);
        }
    }
}
