using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace MccSoft.Testing.SqliteUtils.EFExtensions
{
    public static class SqliteDbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UsePostgresFunctionsInSqlite(
            this DbContextOptionsBuilder optionsBuilder
        )
        {
            return optionsBuilder.ReplaceService<
                IMethodCallTranslatorProvider,
                CustomSqliteMethodCallTranslatorPlugin
            >();
        }
    }
}
