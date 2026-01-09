using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.WebHooks.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MccSoft.WebHooks;

/// <summary>
/// This is to be able to inject IServiceProvider into JobFilter attribute
/// </summary>
/// <param name="serviceProvider"></param>
public class WebHookInitializationHostedService(IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        GlobalJobFilters.Filters.Add(
            serviceProvider.GetRequiredService<WebHookDeliveryJobFailureAttribute>()
        );
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
