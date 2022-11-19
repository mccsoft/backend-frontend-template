using System;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        services.AddTransient<DomainEventsSaveChangesInterceptor>();
        services.AddTransient<DomainEventsTransactionInterceptor>();

        // to resolve all Handlers in a separate scope, or in a HTTP scope
        services.AddTransient<ServiceFactory>(p =>
        {
            var httpContextAccessor = p.GetRequiredService<IHttpContextAccessor>();
            if (httpContextAccessor.HttpContext != null)
            {
                return httpContextAccessor.HttpContext.RequestServices.GetRequiredService;
            }

            var scope = p.CreateScope();
            return scope.ServiceProvider.GetRequiredService;
        });
        services.AddMediatR(handlerAssemblyMarkerTypes);
    }
}
