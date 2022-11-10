using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IWebHookSender
{
    Task SendWebHook(string url, object body);
}

public class WebHookSender : IWebHookSender
{
    private readonly DbContext _dbContext;

    public WebHookSender(DbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task SendWebHook(string url, object body)
    {
        throw new NotImplementedException();
    }
}

public interface IWebHookInterceptor
{
    Task OnExecutionAttempt(WebHook webHook);
    Task OnExecutionSucceeded(WebHook webHook);
    Task OnExecutionFailed(WebHook webHook);
    Task OnAllAttemptsFailed(WebHook webHook);
}

public abstract class WebHookInterceptor : IWebHookInterceptor
{
    public Task OnAllAttemptsFailed(WebHook webHook)
    {
        throw new NotImplementedException();
    }

    public Task OnExecutionAttempt(WebHook webHook)
    {
        throw new NotImplementedException();
    }

    public Task OnExecutionFailed(WebHook webHook)
    {
        throw new NotImplementedException();
    }

    public Task OnExecutionSucceeded(WebHook webHook)
    {
        throw new NotImplementedException();
    }
}

public class WebHookProcessor
{
    private DbContext _dbContext;

    public WebHookProcessor(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CheckAndSend()
    {
        var configuration = new WebHookConfiguration();

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var webHooks = await _dbContext
            .Set<WebHook>()
            .Where(x => !x.IsSucceded && x.NextRun < DateTime.UtcNow)
            .Take(10)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        AdjustNextRun(configuration, webHooks);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        foreach (var webHook in webHooks)
        {
            try
            {
                var result = await ProcessWebHook(webHook);
                webHook.MarkSuccessful(result);
            }
            catch (Exception e)
            {
                webHook.MarkFailed(e);
                
            }
            finally
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task<string> ProcessWebHook(WebHook webHook)
    {
        var client = new HttpClient();
        var message = new HttpRequestMessage(webHook.HttpMethod, webHook.TargetUrl);
        message.Content = new StringContent(webHook.SerializedBody);

        var result = await client.SendAsync(message);

        result.EnsureSuccessStatusCode();

        return await result.Content.ReadAsStringAsync();
    }

    private void AdjustNextRun(WebHookConfiguration configuration, List<WebHook> webHooks)
    {
        var delaysConfiguration = configuration.SendingDelaysInMinutes;
        var now = DateTime.UtcNow;
        foreach (var webHook in webHooks)
        {
            var delayTillNextExecution =
                delaysConfiguration.Length > webHook.AttemptsPerformed
                    ? delaysConfiguration[webHook.AttemptsPerformed]
                    : 0;
            if (delayTillNextExecution == 0)
                continue;

            
                webHook.BeforeRun(DateTime.UtcNow,delayTillNextExecution);
            
        }
    }
}
