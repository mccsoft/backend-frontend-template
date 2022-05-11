using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DatabaseInitialization;

public record BasicDatabaseSeedingOptions<TDbContext>(
    string Name,
    Func<TDbContext, Task> SeedingFunction,
    Func<DbContextOptions<TDbContext>, TDbContext>? DbContextFactory = null
) where TDbContext : DbContext;
