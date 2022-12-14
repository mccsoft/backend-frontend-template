using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MccSoft.NpgSql;

public static class NpgsqlDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder EnableRetryOnFailureWithAdditionalErrorCodes(
        this NpgsqlDbContextOptionsBuilder builder,
        int? maxRetryCount = null,
        TimeSpan? maxRetryDelay = null,
        IList<string> errorCodesToAdd = null
    )
    {
        // default values are taken from Npgsql sources
        maxRetryCount ??= 6;
        maxRetryDelay ??= TimeSpan.FromSeconds(30);
        errorCodesToAdd ??= new List<string>();

        return builder.EnableRetryOnFailure(
            maxRetryCount.Value,
            maxRetryDelay.Value,
            PostgresTransientErrorCodes.Union(errorCodesToAdd).ToList()
        );
    }

    /// <summary>
    /// These codes were got from <see cref="PostgresException.IsTransient">PostgresException.IsTransient</see>.
    /// These are not used by <see cref="NpgsqlRetryingExecutionStrategy"/> by default,
    /// but it's recommended to retry transaction when you get exception with these codes.
    /// </summary>
    private static readonly string[] PostgresTransientErrorCodes =
    {
        PostgresErrorCodes.InsufficientResources,
        PostgresErrorCodes.DiskFull,
        PostgresErrorCodes.OutOfMemory,
        PostgresErrorCodes.TooManyConnections,
        PostgresErrorCodes.ConfigurationLimitExceeded,
        PostgresErrorCodes.CannotConnectNow,
        PostgresErrorCodes.SystemError,
        PostgresErrorCodes.IoError,
        PostgresErrorCodes.SerializationFailure,
        PostgresErrorCodes.DeadlockDetected,
        PostgresErrorCodes.LockNotAvailable,
        PostgresErrorCodes.ObjectInUse,
        PostgresErrorCodes.ObjectNotInPrerequisiteState,
        PostgresErrorCodes.ConnectionException,
        PostgresErrorCodes.ConnectionDoesNotExist,
        PostgresErrorCodes.ConnectionFailure,
        PostgresErrorCodes.SqlClientUnableToEstablishSqlConnection,
        PostgresErrorCodes.SqlServerRejectedEstablishmentOfSqlConnection,
        PostgresErrorCodes.TransactionResolutionUnknown,
        PostgresErrorCodes.UniqueViolation,
        PostgresErrorCodes.CheckViolation,
    };
}
