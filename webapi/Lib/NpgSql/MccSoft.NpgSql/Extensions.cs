using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.NpgSql
{
    public static class Extensions
    {
        public static void RegisterRetryHelper(this IServiceCollection services)
        {
            services.AddScoped(typeof(TransactionLogger<>))
                .AddScoped(typeof(PostgresRetryHelper<, >));
        }
    }
}
