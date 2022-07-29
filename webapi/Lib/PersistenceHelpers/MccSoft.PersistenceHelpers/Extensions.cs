using MccSoft.NpgSql;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.PersistenceHelpers
{
    public static class Extensions
    {
        public static void RegisterRetryHelper(this IServiceCollection services)
        {
            services
                .AddScoped(typeof(TransactionLogger<>))
                .AddScoped(typeof(DbRetryHelper<,>));
        }
  }
}
