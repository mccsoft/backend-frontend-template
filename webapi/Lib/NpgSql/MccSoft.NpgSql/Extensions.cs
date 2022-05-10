using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MccSoft.NpgSql
{
    public static class Extensions
    {
        public static void RegisterRetryHelper(this IServiceCollection services)
        {
            services
                .AddScoped(typeof(TransactionLogger<>))
                .AddScoped(typeof(PostgresRetryHelper<,>));
        }

        public static void ReloadTypesForEnumSupport(this DbContext context)
        {
            var conn = (NpgsqlConnection)context.Database.GetDbConnection();
            conn.Open();
            conn.ReloadTypes();
        }
    }
}
