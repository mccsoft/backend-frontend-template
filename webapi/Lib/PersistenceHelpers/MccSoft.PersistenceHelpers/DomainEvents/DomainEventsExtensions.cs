﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.PersistenceHelpers.DomainEvents;

public static class DomainEventsExtensions
{
    public static DbContextOptionsBuilder AddDomainEventsInterceptors(
        this DbContextOptionsBuilder dbContextOptionsBuilder,
        IServiceProvider provider
    )
    {
        return dbContextOptionsBuilder.AddInterceptors(
            provider.GetRequiredService<DomainEventsSaveChangesInterceptor>(),
            provider.GetRequiredService<DomainEventsTransactionInterceptor>()
        );
    }

    public static void AddDomainEventsWithMediatR(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes
    )
    {
        services.AddDomainEventsWithMediatR(config =>
        {
            foreach (var typeToRegister in handlerAssemblyMarkerTypes)
            {
                config.RegisterServicesFromAssemblyContaining(typeToRegister);
            }
        });
    }

    public static void AddDomainEventsWithMediatR(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration> configure
    )
    {
        services.AddTransient<DomainEventsSaveChangesInterceptor>();
        services.AddTransient<DomainEventsTransactionInterceptor>();
        services.AddMediatR(configure);
    }
}
