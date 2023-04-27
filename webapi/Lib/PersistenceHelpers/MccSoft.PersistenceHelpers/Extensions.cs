using System;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.PersistenceHelpers;

public static class Extensions
{
    public static void RegisterRetryHelper(
        this IServiceCollection services,
        Action<DbRetryHelperOptions>? setupAction = null
    )
    {
        services.Configure(setupAction ?? (_ => { }));
        services.AddScoped(typeof(TransactionLogger<>)).AddScoped(typeof(DbRetryHelper<,>));
    }
}
