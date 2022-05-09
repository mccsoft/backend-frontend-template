using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public interface IDatabaseInitializer : IDisposable
{
    /// <summary>
    /// Returns a connection string to be used in the test.
    /// Under the cover it creates a template database (using <paramref name="initializeDatabase"/> function)
    /// and then create a copy of it to be used in each test.
    /// </summary>
    /// <param name="databaseHash">Hash that uniquely identifies your database structure + seed data</param>
    /// <param name="initializeDatabase">
    /// Function that should create DB schema and seed the data.
    /// Receives a connection string.
    /// </param>
    /// <returns></returns>
    Task<string> GetConnectionString(string databaseHash, Func<string, Task> initializeDatabase);

    /// <summary>
    /// Returns a connection string for passed DbContext to be used in the test.
    /// Under the cover it creates a template database using DbContext.Database.EnsureCreated and
    /// runs a <paramref name="databaseSeeding"/> on it.
    /// Then it creates a copy of template database to be used in each test.
    /// </summary>
    /// <typeparam name="TDbContext">
    /// DbContext that is used to create a template database (by running DbContext.Database.EnsureCreated()
    /// </typeparam>
    Task<string> GetConnectionStringUsingEnsureCreated<TDbContext>(
        BasicDatabaseSeedingOptions<TDbContext> databaseSeeding
    ) where TDbContext : DbContext;

    /// <summary>
    /// Returns test database to a pool
    /// </summary>
    Task ReturnDatabase(string connectionString);

    /// <summary>
    /// Calls options.UseNpgsql(<paramref name="connectionString"/>) or options.UseSqlite(<paramref name="connectionString"/>)
    /// depending on database provider initializer works with.
    /// Helps build the common WebApplicationFactory creation code which only requires IDatabaseInitializer.
    /// </summary>
    void UseProvider(DbContextOptionsBuilder options, string connectionString);

    /// <summary>
    /// Creates connectionString using <see cref="GetConnectionStringUsingEnsureCreated"/>.
    /// Calls options.UseNpgsql() or options.UseSqlite() depending on database provider initializer works with.
    /// Helps build the common WebApplicationFactory creation code which only requires IDatabaseInitializer
    /// </summary>
    void UseProvider<TDbContext>(
        DbContextOptionsBuilder options,
        BasicDatabaseSeedingOptions<TDbContext> databaseSeedingOptions
    ) where TDbContext : DbContext;
}
