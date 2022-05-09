using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public class ContextHelper
{
    /// <summary>
    /// Returns the name of the last DB migration in passed DbContext.
    /// Tries to create the DbContext using the passed factory method,
    /// or by using a constructor with DbContextOptions as a first argument and nulls as all the rest.
    /// </summary>
    public static string GetLastMigrationName<T>(Func<DbContextOptions<T>, T> factoryMethod = null)
        where T : DbContext
    {
        var dbContext = CreateDbContext(factoryMethod);
        var assemblyMigrations = dbContext.Database.GetMigrations();
        return assemblyMigrations.Last();
    }

    internal static T CreateDbContext<T>(
        Func<DbContextOptions<T>, T> factoryMethod = null,
        string connectionString = "Server=any"
    ) where T : DbContext
    {
        factoryMethod ??= (options) =>
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), options);
            }
            catch (MissingMethodException)
            {
                try
                {
                    return (T)Activator.CreateInstance(typeof(T), options, null);
                }
                catch (MissingMethodException)
                {
                    return (T)Activator.CreateInstance(typeof(T), options, null, null);
                }
            }
        };

        var dbContext = factoryMethod(
            new DbContextOptionsBuilder<T>().UseNpgsql(connectionString).Options
        );
        return dbContext;
    }
}
