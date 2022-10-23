using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.Testing.Database;

public static class RegistrationExtensions
{
    public static void RemoveDbContextRegistration<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.RemoveRegistration<DbContextOptions<TDbContext>>();
    }

    public static void RemoveRegistration<TService>(this IServiceCollection services)
    {
        var descriptor = services.Single(d => d.ServiceType == typeof(TService));
        services.Remove(descriptor);
    }

    public static void RemoveIfExists<TService>(this IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor != null)
            services.Remove(descriptor);
    }
}
