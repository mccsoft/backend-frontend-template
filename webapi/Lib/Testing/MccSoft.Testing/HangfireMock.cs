using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MccSoft.Testing;

public static class HangfireMock
{
    public static IServiceCollection RegisterHangfireMock(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IBackgroundJobClient>(s =>
            s.CreateHangfireMock().Object
        );
    }

    /// <summary>
    /// Creates mock for IBackgroundJobClient that runs all fire-and-forget jobs synchronously
    /// (i.e. if you do
    /// backgroundJobClient.Enqueue(x => Console.WriteLine("zxc"))
    /// it runs Console.WriteLine immediately).
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve classes from</param>
    public static Mock<IBackgroundJobClient> CreateHangfireMock(
        this IServiceProvider serviceProvider
    )
    {
        return CreateHangfireMock(() => serviceProvider);
    }

    /// <summary>
    /// Creates mock for IBackgroundJobClient that runs all fire-and-forget jobs synchronously
    /// (i.e. if you do
    /// backgroundJobClient.Enqueue(x => Console.WriteLine("zxc"))
    /// it runs Console.WriteLine immediately).
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve classes from</param>
    public static Mock<IBackgroundJobClient> CreateHangfireMock(
        Func<IServiceProvider> serviceProvider
    )
    {
        var backgroundJobClient = new Mock<IBackgroundJobClient>();
        backgroundJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback(
                (Job job, IState state) =>
                {
                    using var scope = serviceProvider().CreateScope();
                    object? result;
                    if (job.Method.IsStatic)
                    {
                        result = job.Method.Invoke(null, job.Args.ToArray());
                    }
                    else
                    {
                        var service = scope.ServiceProvider.GetRequiredService(job.Type);
                        result = job.Method.Invoke(service, job.Args.ToArray());
                    }
                    if (result is Task resultTask)
                    {
                        resultTask.GetAwaiter().GetResult();
                    }
                }
            );
        return backgroundJobClient;
    }
}
