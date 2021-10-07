using System;
using MccSoft.Testing.SqliteUtils.EFExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NeinLinq;

namespace MccSoft.Testing.SqliteUtils
{
    public static class SqliteServiceExtensions
    {
        /// <summary>
        /// Configures the application to use an Sqlite in-memory database.
        /// Used in component tests.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the DB context.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="dbId">
        /// A unique identifier of the DB to create. Usually, a random string.
        /// </param>
        public static void AddSqliteInMemory<TDbContext>(
            this IServiceCollection services,
            string dbId
        ) where TDbContext : DbContext
        {
            string connectionString = $"DataSource=file:memdb{dbId}?mode=memory&cache=shared";
            services.AddSingleton(
                new SqliteConnectionHolder.ConnectionString { Str = connectionString }
            );
            services.AddSingleton<SqliteConnectionHolder>();

            // Cannot use a shared Sqlite connection as other tests do, because we create
            // transactions in parallel threads, and Sqlite fails with error
            // "SqliteConnection does not support nested transactions".
            services.AddDbContext<TDbContext>(
                options =>
                {
                    options.UseSqlite(connectionString)
                        .UsePostgresFunctionsInSqlite()
                        .WithLambdaInjection()
                        .ReplaceService<
                            IModelCustomizer,
                            ModelCustomizerWithPatchedDateTimeOffset
                        >();
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Singleton
            );
        }

        /// <summary>
        /// Creates the in-memory DB that was configured by
        /// <see cref="AddSqliteInMemory{TDbContext}"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the DB context.</typeparam>
        /// <param name="container">The DI container.</param>
        public static void InitializeSqliteDb<TDbContext>(this IServiceProvider container)
            where TDbContext : DbContext
        {
            // Initialize the long-living Sqlite connection.
            var _ = container.GetRequiredService<SqliteConnectionHolder>();
            using (IServiceScope scope = container.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
                context.Database.EnsureCreated();
            }
        }
    }
}
