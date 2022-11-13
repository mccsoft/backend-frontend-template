using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class WebHookRegistration
{
    public static Type DbContextType { get; private set; }

    public static DbSet<WebHook> WebHooks(this DbContext dbContext) => dbContext.Set<WebHook>();

    public static ModelBuilder AddWebHookEntities(
        this ModelBuilder builder,
        System.Type dbContextType
    )
    {
        DbContextType = dbContextType;
        builder.Entity<WebHook>(e =>
        {
            e.Property(x => x.Headers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v =>
                        JsonSerializer.Deserialize<Dictionary<string, string>>(
                            v,
                            (JsonSerializerOptions)null
                        )
                );
            e.OwnsOne(x => x.AdditionalData);
        });
        return builder;
    }

    public static IServiceCollection AddWebHooks(
        this IServiceCollection serviceCollection,
        Action<WebHookConfiguration>? configureOptions = null
    )
    {
        var configuration = new WebHookConfiguration();
        configureOptions?.Invoke(configuration);

        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddTransient<WebHookSender>();
        serviceCollection.AddTransient<WebHookProcessor>();
        serviceCollection.AddTransient<WebHookProcessor>();
        serviceCollection.AddTransient<IWebHookSender, WebHookSender>();

        return serviceCollection;
    }
}
