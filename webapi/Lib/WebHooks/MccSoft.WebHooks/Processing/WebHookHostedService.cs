using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class WebHookHostedService : BackgroundService
{
    private TaskCompletionSource _taskCompletionSource = new TaskCompletionSource();
    private readonly IServiceProvider _serviceProvider;
    private readonly WebHookConfiguration _configuration;

    public WebHookHostedService(
        IServiceProvider serviceProvider,
        WebHookConfiguration configuration
    )
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            await Execute(scope.ServiceProvider, stoppingToken);
            await Task.Delay(_configuration.PollingDelay, stoppingToken);
        }
        _taskCompletionSource.TrySetResult();
    }

    private async Task Execute(IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        var service = serviceProvider.GetRequiredService<WebHookProcessor>();
        await service.RunJob(stoppingToken);
    }
}
