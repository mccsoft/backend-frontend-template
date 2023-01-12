using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers;

/// <summary>
/// Configures EntityFrameworkCore logging.
/// </summary>
public static class EfLogHelper
{
    /// <summary>
    /// Configures the EF logger so that some errors are logged as warnings to avoid unnecessary alerts.
    /// </summary>
    /// <remarks>
    /// When the configured events happen and cause a real error (not a retry), we produce an error record
    /// ourselves, and this record is enough to alert us, the ones produced by EF are unnecessary.
    /// </remarks>
    /// <param name="builder">An object that configures the DB context.</param>
    public static void ReduceLogLevel(DbContextOptionsBuilder builder)
    {
        // The following EF core EventIds are logged at LogLevel.Error by default:
        // - Microsoft.EntityFrameworkCore.Update.SaveChangesFailed
        // - Microsoft.EntityFrameworkCore.Database.Command.CommandError
        // - Microsoft.EntityFrameworkCore.Database.Transaction.TransactionError
        // - Microsoft.EntityFrameworkCore.Query.QueryIterationFailed
        // (These are the names, we refer to them by numeric ids.)
        builder.ConfigureWarnings(
            c =>
                c.Log(
                    (RelationalEventId.CommandError, LogLevel.Warning),
                    (RelationalEventId.TransactionError, LogLevel.Warning),
                    (CoreEventId.SaveChangesFailed, LogLevel.Warning),
                    (CoreEventId.QueryIterationFailed, LogLevel.Warning)
                )
        );
    }
}
