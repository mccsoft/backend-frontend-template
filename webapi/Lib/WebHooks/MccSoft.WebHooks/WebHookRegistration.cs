using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class WebHookRegistration
{
    public static Type DbContextType {get; private set;}
    public static ModelBuilder AddWebHookEntities(this ModelBuilder builder, System.Type dbContextType)
    {
        DbContextType = dbContextType;
        builder.Entity<WebHook>();
        return builder;
    }

    public static IServiceCollection AddWebHooks(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<WebHookSender>();

        return serviceCollection;
    }
}
