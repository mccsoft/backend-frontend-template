using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MccSoft.Testing.SqliteUtils.EFExtensions
{
    public static class SqliteDbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UsePostgresFunctionsInSqlite(
            this DbContextOptionsBuilder optionsBuilder
        ) {
            var extension = GetOrCreateExtension(optionsBuilder);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                extension
            );

            return optionsBuilder;
        }

        private static SqliteDbContextOptionsExtension GetOrCreateExtension(
            DbContextOptionsBuilder optionsBuilder
        ) =>
            optionsBuilder.Options.FindExtension<SqliteDbContextOptionsExtension>()
            ?? new SqliteDbContextOptionsExtension();
    }
}
