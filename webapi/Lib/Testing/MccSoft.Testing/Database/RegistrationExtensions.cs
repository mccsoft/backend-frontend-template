using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.Testing.Database;

public static class RegistrationExtensions
{
    public static void RemoveDbContextRegistration<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptor = services.Single(
            d => d.ServiceType == typeof(DbContextOptions<TDbContext>)
        );
        services.Remove(descriptor);
    }
}
