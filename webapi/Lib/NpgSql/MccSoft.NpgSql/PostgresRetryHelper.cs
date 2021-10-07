using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MccSoft.NpgSql
{
    /// <summary>
    /// Retries a failed Postgres operation on a transient error.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    /// <typeparam name="TCaller">The type of the service performing transaction.</typeparam>
    public class PostgresRetryHelper<TDbContext, TCaller>
        where TDbContext : DbContext, ITransactionFactory, IDisposable
    {
        private readonly Func<TDbContext> _dbContextFactory;
        private readonly TransactionLogger<TCaller> _transactionLogger;
        private readonly ILogger _logger;
        private const int _retryLimit = 5;

        public PostgresRetryHelper(
            Func<TDbContext> dbContextFactory,
            ILoggerFactory loggerFactory,
            TransactionLogger<TCaller> transactionLogger
        ) {
            _dbContextFactory = dbContextFactory;
            _transactionLogger = transactionLogger;
            _logger = loggerFactory.CreateLogger<PostgresRetryHelper<TDbContext, TCaller>>();
        }

        /// <summary>
        /// Retries the specified action on transient Postgres errors with exponential backoff.
        /// Wraps the action in a transaction that is committed automatically
        /// if the action doesn't throw and returns the result of the action.
        /// </summary>
        /// <param name="action">The action that may result in a transient error.</param>
        public async Task<TResult> RetryInTransactionAsync<TResult>(
            Func<TDbContext, ILogger, Task<TResult>> action
        ) {
            TResult result = default;
            bool failed;
            int retries = 0;

            do
            {
                failed = false;
                try
                {
                    using (TDbContext db = _dbContextFactory())
                    {
                        IExecutionStrategy strategy = db.Database.CreateExecutionStrategy();
                        await strategy.Execute(
                            async () =>
                            {
                                await using IDbContextTransaction transaction =
                                    await db.BeginTransactionAsync();
                                result = await action(db, _transactionLogger);
                                await transaction.CommitAsync();
                            }
                        );
                    }
                    //using (IDbContextTransaction transaction = await db.BeginTransactionAsync())
                    //{
                    //    result = await action(db, _transactionLogger);
                    //    transaction.Commit();
                    //}
                }
                catch (Exception ex)
                    when (retries < _retryLimit && TransientHelper.IsTransientPostgresError(ex))
                {
                    failed = true;
                    retries++;
                    _logger.LogWarning(
                        ex,
                        "Retrying on a transient error. Retry count: " + retries
                    );

                    // Delay grows exponentially: 5, 25, 125, ... 3125ms.
                    var sleepTime = TimeSpan.FromMilliseconds(Math.Pow(5.0, retries));
                    await Task.Delay(sleepTime);

                    _transactionLogger.Fail();
                }
            } while (failed);

            _transactionLogger.Succeed();

            return result;
        }

        /// <summary>
        /// Retries the specified action on transient Postgres errors with exponential backoff.
        /// Wraps the action in a transaction that is committed automatically
        /// if the action doesn't throw.
        /// </summary>
        /// <param name="action">The action that may result in a transient error.</param>
        public async Task RetryInTransactionAsync(Func<TDbContext, ILogger, Task> action)
        {
            await RetryInTransactionAsync(
                async (dbContext, transactionLogger) =>
                {
                    await action(dbContext, transactionLogger);
                    return 0;
                }
            );
        }

        /// <summary>
        /// Retries the specified action on transient Postgres errors with exponential backoff.
        /// Wraps the action in a transaction that is committed automatically
        /// if the action doesn't throw and returns the result of the action.
        /// </summary>
        /// <param name="action">The action that may result in a transient error.</param>
        public async Task<TResult> RetryInTransactionAsync<TResult>(
            Func<TDbContext, Task<TResult>> action
        ) {
            return await RetryInTransactionAsync((dbContext, _) => action(dbContext));
        }

        /// <summary>
        /// Retries the specified action on transient Postgres errors with exponential backoff.
        /// Wraps the action in a transaction that is committed automatically
        /// if the action doesn't throw.
        /// </summary>
        /// <param name="action">The action that may result in a transient error.</param>
        public async Task RetryInTransactionAsync(Func<TDbContext, Task> action)
        {
            await RetryInTransactionAsync(
                async (dbContext) =>
                {
                    await action(dbContext);
                    return 0;
                }
            );
        }
    }
}
