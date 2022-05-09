﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public abstract class BaseDatabaseInitializer : IDatabaseInitializer
{
    public abstract Task<string> GetConnectionString(
        string databaseHash,
        Func<string, Task> initializeDatabase
    );

    public abstract Task ReturnDatabase(string connectionString);

    public abstract void UseProvider(DbContextOptionsBuilder options, string connectionString);

    public void UseProvider<TDbContext>(
        DbContextOptionsBuilder options,
        BasicDatabaseSeedingOptions<TDbContext> databaseSeedingOptions
    ) where TDbContext : DbContext
    {
        string connectionString = GetConnectionStringUsingEnsureCreated(databaseSeedingOptions)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        UseProvider(options, connectionString);
    }

    public Task<string> GetConnectionStringUsingEnsureCreated<TDbContext>(
        BasicDatabaseSeedingOptions<TDbContext> databaseSeeding
    ) where TDbContext : DbContext
    {
        string lastMigrationName = ContextHelper.GetLastMigrationName<TDbContext>();

        return GetConnectionString(
            databaseSeeding?.Name
                + nameof(GetConnectionStringUsingEnsureCreated)
                + lastMigrationName
                + typeof(TDbContext).Assembly.FullName,
            async (connectionString) =>
            {
                var dbContext = ContextHelper.CreateDbContext<TDbContext>(
                    connectionString: connectionString
                );
                await (databaseSeeding?.SeedingFunction?.Invoke(dbContext) ?? Task.CompletedTask);
            }
        );
    }

    public virtual void Dispose() { }
}
