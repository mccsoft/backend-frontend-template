using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace MccSoft.Logging;

/// <summary>
/// Extension methods to format/add
/// additional information to logs,
/// that are being sent to Serilog.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Begins a logging scope at the system boundary,
    /// adding SessionId, TraceId, SpanId and ParentId to log messages.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="activityName">
    /// The name to initialize the <see cref="Activity"/> with.
    /// </param>
    /// <param name="sessionId">The id of the session the operation is part of.</param>
    /// <param name="additionalParams">Additional properties to add to all messages.</param>
    /// <returns>An object to be disposed when the operation ends.</returns>
    public static IDisposable BeginTopLevelActivity(
        this ILogger logger,
        string activityName,
        string sessionId,
        params KeyValuePair<string, object>[] additionalParams
    )
    {
        return new TopLevelActivityScope(
            logger,
            activityName,
            sessionId,
            additionalParams.ToList()
        );
    }

    /// <summary>
    /// Begins a global activity to log application startup messages with.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public static void StartGlobalRootActivity(this ILogger logger)
    {
        logger.BeginTopLevelActivity("WebHost", sessionId: null);
    }

    #region LogOperation via Disposable

    /// <summary>
    /// Logs operation.
    /// Dispose the result when operation is finished (e.g. by wrapping the result in a `using` statement)
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static IDisposable LogOperation(
        this ILogger logger,
        OperationContext context,
        [CallerMemberName] string operationName = ""
    )
    {
        return LogOperation(logger, context, null, operationName);
    }

    /// <summary>
    /// Should only be used from within LogAttribute!!!
    /// Logs operation.
    /// Dispose the result when operation is finished (e.g. by wrapping the result in a `using` statement)
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="resultFunction">
    /// Function returning the result of executing a function.
    /// The function will be executed and the result will be logged when function finishes.
    /// </param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static IDisposable LogOperationForAttribute(
        this ILogger logger,
        OperationContext context,
        Func<object> resultFunction = null,
        [CallerMemberName] string operationName = ""
    )
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var loggingScope = logger.BeginScope($"Operation {Field.Method}", operationName);
        var loggingContext = LogContext.Push(new OperationContextEnricher(context));

        logger.LogDebug($"Starting operation {Field.Method}.", operationName);

        return new Disposable(() =>
        {
            try
            {
                stopwatch.Stop();
                if (resultFunction != null)
                {
                    var result = resultFunction.Invoke();
                    logger.LogWithFields(
                        LogLevel.Information,
                        context,
                        $"Finished operation {Field.Method}. Result: {Field.Result}. Elapsed: {Field.Elapsed} ms.",
                        operationName,
                        result,
                        stopwatch.ElapsedMilliseconds
                    );
                }
                else
                {
                    logger.LogWithFields(
                        LogLevel.Information,
                        context,
                        $"Finished operation {Field.Method}. Elapsed: {Field.Elapsed} ms.",
                        operationName,
                        stopwatch.ElapsedMilliseconds
                    );
                }
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                logger.LogError(
                    e,
                    $"Operation {Field.Method} failed after {Field.Elapsed} ms.",
                    operationName,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }

            loggingScope.Dispose();
            loggingContext.Dispose();
        });
    }

    #endregion

    /// <summary>
    /// Logs a message with additional parameters appended in the format "name: value".
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="level">The log level.</param>
    /// <param name="fields">
    /// The fields to append to the message in a structured way.
    /// </param>
    /// <param name="message">
    /// The message to log. Can contain placeholders for structured logging.
    /// </param>
    /// <param name="args">
    /// Array of objects that correspond to placeholders in <paramref name="message"/>
    /// </param>
    public static void LogWithFields(
        this ILogger logger,
        LogLevel level,
        Dictionary<Field, object> fields,
        string message,
        params object[] args
    )
    {
        ArgumentNullException.ThrowIfNull(fields);

        var msg = new StringBuilder(message);

        var argsIndex = args.Length;
        Array.Resize(ref args, args.Length + fields.Count);

        foreach (var (field, value) in fields)
        {
            msg.Append($" {field.Name}: {field}");
            args[argsIndex++] = value;
        }

        logger.Log(level, msg.ToString(), args);
    }

    #region Async LogOperation

    /// <summary>
    /// Logs operation.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static async Task LogOperation(
        this ILogger logger,
        OperationContext context,
        Func<Task> action,
        [CallerMemberName] string operationName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(action);

        Func<Task<int>> func = async () =>
        {
            await action();
            return 0;
        };

        await logger.LogOperation(context, func, operationName);
    }

    /// <summary>
    /// Logs operation with additional parameters in the start message.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="startParams">
    /// The additional parameters to log in the start message only.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static async Task LogOperation(
        this ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Func<Task> action,
        [CallerMemberName] string operationName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(action);

        Func<Task<int>> func = async () =>
        {
            await action();
            return 0;
        };

        await logger.LogOperation(context, startParams, func, operationName);
    }

    /// <summary>
    /// Logs operation.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static async Task<T> LogOperation<T>(
        this ILogger logger,
        OperationContext context,
        Func<Task<T>> action,
        [CallerMemberName] string operationName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(action);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (logger.BeginScope($"Operation {Field.Method}", operationName))
        using (LogContext.Push(new OperationContextEnricher(context)))
        {
            T result;
            {
                logger.LogDebug($"Starting operation {Field.Method}.", operationName);
                result = await action();
                stopwatch.Stop();
                logger.LogWithFields(
                    LogLevel.Information,
                    context,
                    $"Finished operation {Field.Method}. Elapsed: {Field.Elapsed} ms.",
                    operationName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
    }

    /// <summary>
    /// Logs operation with additional parameters in the start message.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="startParams">
    /// The additional parameters to log in the start message only.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static async Task<T> LogOperation<T>(
        this ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Func<Task<T>> action,
        [CallerMemberName] string operationName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(startParams);
        ArgumentNullException.ThrowIfNull(action);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (logger.BeginScope($"Operation {Field.Method}", operationName))
        using (LogContext.Push(new OperationContextEnricher(context)))
        {
            T result;
            {
                logger.LogDebug($"Starting operation {Field.Method}.", operationName);
                result = await action();
                stopwatch.Stop();
                logger.LogWithFields(
                    LogLevel.Information,
                    startParams,
                    $"Finished operation {Field.Method}. Elapsed: {Field.Elapsed} ms.",
                    operationName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
    }

    #endregion

    #region Sync LogOperation




    /// <summary>
    /// Logs operation.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static T LogOperation<T>(
        this ILogger logger,
        OperationContext context,
        Func<T> action,
        [CallerMemberName] string operationName = ""
    )
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (logger.BeginScope($"Operation {Field.Method}", operationName))
        using (LogContext.Push(new OperationContextEnricher(context)))
        {
            T result;
            {
                logger.LogDebug($"Starting operation {Field.Method}.", operationName);
                result = action();
                stopwatch.Stop();
                logger.LogWithFields(
                    LogLevel.Information,
                    context,
                    $"Finished operation {Field.Method}. Elapsed: {Field.Elapsed} ms.",
                    operationName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
    }

    /// <summary>
    /// Logs operation.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static void LogOperation(
        this ILogger logger,
        OperationContext context,
        Action action = default,
        [CallerMemberName] string operationName = ""
    )
    {
        Func<int> func = () =>
        {
            action();
            return 1;
        };
        logger.LogOperation(context, func, operationName);
    }

    /// <summary>
    /// Logs operation with additional parameters in the start message.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="startParams">
    /// The additional parameters to log in the start message only.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static T LogOperation<T>(
        this ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Func<T> action,
        [CallerMemberName] string operationName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(startParams);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (logger.BeginScope($"Operation {Field.Method}", operationName))
        using (LogContext.Push(new OperationContextEnricher(context)))
        {
            T result;
            {
                logger.LogDebug($"Starting operation {Field.Method}.", operationName);
                result = action();
                stopwatch.Stop();
                logger.LogWithFields(
                    LogLevel.Information,
                    startParams,
                    $"Finished operation {Field.Method}. Elapsed: {Field.Elapsed} ms.",
                    operationName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
    }

    /// <summary>
    /// Logs operation with additional parameters in the start message.
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="context">
    /// Logging context that would be logged as the scope, for each logged message.
    /// </param>
    /// <param name="startParams">
    /// The additional parameters to log in the start message only.
    /// </param>
    /// <param name="action">Action that would be dispatched in scope of logged context.</param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static void LogOperation(
        this ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Action action = default,
        [CallerMemberName] string operationName = ""
    )
    {
        Func<int> func = () =>
        {
            action();
            return 1;
        };
        logger.LogOperation(context, startParams, func, operationName);
    }

    #endregion
}
