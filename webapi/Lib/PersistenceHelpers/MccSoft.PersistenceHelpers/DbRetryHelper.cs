using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MccSoft.PersistenceHelpers;

/// <summary>
/// Retries a failed database operation on a transient error.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TCaller">The type of the service performing transaction.</typeparam>
public class DbRetryHelper<TDbContext, TCaller> where TDbContext : DbContext, IDisposable
{
    #region Constructor and dependencies

    private readonly TDbContext _dbContext;
    private readonly Func<TDbContext> _dbContextFactory;
    private readonly TransactionLogger<TCaller> _transactionLogger;
    private readonly IOptions<DbRetryHelperOptions> _options;

    public DbRetryHelper(
        TDbContext dbContext,
        Func<TDbContext> dbContextFactory,
        TransactionLogger<TCaller> transactionLogger,
        IOptions<DbRetryHelperOptions> options
    )
    {
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _transactionLogger = transactionLogger;
        _options = options;
    }

    #endregion

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Wraps the action in a transaction that is committed automatically
    /// if the action doesn't throw and returns the result of the action.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task<TResult> RetryInTransactionAsync<TResult>(
        Func<TDbContext, ILogger, Task<TResult>> action
    )
    {
        TResult result = default;

        IExecutionStrategy strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using TDbContext db = _dbContextFactory();
            await using IDbContextTransaction transaction = _options.Value.IsolationLevel
                is { } isolationLevel
                ? await db.Database.BeginTransactionAsync(isolationLevel)
                : await db.Database.BeginTransactionAsync();
            result = await action(db, _transactionLogger);
            await transaction.CommitAsync();
        });

        _transactionLogger.Succeed();

        return result;
    }

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Doesn't wrap the whole action in a transaction automatically!
    /// Use when you have a single SaveChanges inside the action.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task<TResult> RetryWithoutTransactionAsync<TResult>(
        Func<TDbContext, ILogger, Task<TResult>> action
    )
    {
        TResult result = default;

        IExecutionStrategy strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using TDbContext db = _dbContextFactory();
            result = await action(db, _transactionLogger);
        });

        _transactionLogger.Succeed();

        return result;
    }

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
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
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Doesn't wrap the whole action in a transaction automatically!
    /// Use when you have a single SaveChanges inside the action.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task RetryWithoutTransactionAsync(Func<TDbContext, ILogger, Task> action)
    {
        await RetryWithoutTransactionAsync(
            async (dbContext, transactionLogger) =>
            {
                await action(dbContext, transactionLogger);
                return 0;
            }
        );
    }

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Wraps the action in a transaction that is committed automatically
    /// if the action doesn't throw and returns the result of the action.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task<TResult> RetryInTransactionAsync<TResult>(
        Func<TDbContext, Task<TResult>> action
    )
    {
        return await RetryInTransactionAsync((dbContext, _) => action(dbContext));
    }

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Doesn't wrap the whole action in a transaction automatically!
    /// Use when you have a single SaveChanges inside the action.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task<TResult> RetryWithoutTransactionAsync<TResult>(
        Func<TDbContext, Task<TResult>> action
    )
    {
        return await RetryWithoutTransactionAsync((dbContext, _) => action(dbContext));
    }

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
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

    /// <summary>
    /// Retries the specified action on transient database errors with exponential backoff.
    /// Wraps the action in a transaction that is committed automatically
    /// if the action doesn't throw.
    /// </summary>
    /// <param name="action">The action that may result in a transient error.</param>
    public async Task RetryWithoutTransactionAsync(Func<TDbContext, Task> action)
    {
        await RetryWithoutTransactionAsync(
            async (dbContext) =>
            {
                await action(dbContext);
                return 0;
            }
        );
    }
}

public class DbRetryHelperOptions
{
    /// <summary>
    /// Transaction isolation level, that will be used in methods with transactions
    /// </summary>
    /// <value>
    /// <c>null</c> - default isolation level will be used
    /// </value>
    public IsolationLevel? IsolationLevel { get; set; }
}
