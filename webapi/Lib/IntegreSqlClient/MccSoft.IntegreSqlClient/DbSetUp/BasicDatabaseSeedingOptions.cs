using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public record BasicDatabaseSeedingOptions<TDbContext>(
    string Name,
    Func<TDbContext, Task> SeedingFunction
) where TDbContext : DbContext;
