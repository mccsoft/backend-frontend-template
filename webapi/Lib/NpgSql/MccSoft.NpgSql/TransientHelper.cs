using Npgsql;
using System;

namespace MccSoft.NpgSql;

public static class TransientHelper
{
    /// <summary>
    /// Checks if the exception is a transient exception.
    /// </summary>
    public static bool IsTransientPostgresError(Exception ex)
    {
        // When the transient exception happens in SaveChanges and not in transaction.Commit
        // it is wrapped in several layers of "noisy" exceptions. So we unwrap it.
        do
        {
            if (ex is PostgresException pex)
            {
                return pex.IsTransient;
            }

            ex = ex.InnerException;
        } while (ex != null);

        return false;
    }
}
