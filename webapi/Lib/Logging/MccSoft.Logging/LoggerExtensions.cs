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
        this Microsoft.Extensions.Logging.ILogger logger,
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
    public static void StartGlobalRootActivity(this Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.BeginTopLevelActivity("WebHost", sessionId: null);
    }

    /// <summary>
    /// Logs operation.
    /// Dispose the result when operation is finished (e.g. by wrapping the result in a `using` statement)
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="parameters">
    /// Parameters that should be logged
    /// </param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    public static IDisposable LogOperation(
        this ILogger logger,
        Dictionary<Field, object> parameters,
        [CallerMemberName] string operationName = ""
    )
    {
        return LogOperation(logger, parameters, null, operationName);
    }

    /// <summary>
    /// Logs operation.
    /// Dispose the result when operation is finished (e.g. by wrapping the result in a `using` statement)
    /// </summary>
    /// <param name="logger">
    /// Instance of <see cref="ILogger" />
    /// </param>
    /// <param name="parameters">
    /// Parameters that should be logged
    /// </param>
    /// <param name="resultFunction">
    /// Function returning the result of executing a function.
    /// The function will be executed and the result will be logged when function finishes.
    /// </param>
    /// <param name="operationName">Name of the operation that should be logged.</param>
    internal static IDisposable LogOperation(
        this ILogger logger,
        Dictionary<Field, object> parameters,
        Func<object> resultFunction = null,
        [CallerMemberName] string operationName = ""
    )
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var logContext = LogContext.PushProperty(Field.Method.Name, operationName);

        logger.LogWithFields($"Starting operation {operationName}.", parameters);

        return new Disposable(() =>
        {
            var result = resultFunction?.Invoke();
            stopwatch.Stop();
            logger.LogInformation(
                $"Finished operation {operationName}. Result: {Field.Result}. Elapsed: {Field.Elapsed} ms.",
                result,
                stopwatch.ElapsedMilliseconds
            );
            logContext.Dispose();
        });
    }

    /// <summary>
    /// Logs a message with additional parameters appended in the format "name: value".
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="message">
    /// The message to log. Cannot contain placeholders for structured logging.
    /// </param>
    /// <param name="params">
    /// The parameters to append to the message in a structured way.
    /// </param>
    /// <param name="level">The log level.</param>
    public static void LogWithFields(
        this Microsoft.Extensions.Logging.ILogger logger,
        string message,
        Dictionary<Field, object> @params,
        LogLevel level = LogLevel.Information
    )
    {
        if (@params == null)
        {
            throw new ArgumentNullException(nameof(@params));
        }

        var msg = new StringBuilder(message);
        var paramList = new List<object>(@params.Count);
        foreach ((Field fieldName, object value) in @params)
        {
            msg.Append($" {fieldName.Name}: {{{fieldName.PrefixedName}}}");
            paramList.Add(value);
        }

        logger.Log(level, msg.ToString(), paramList.ToArray());
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
    public static async Task LogOperation(
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Func<Task> action,
        [CallerMemberName] string operationName = ""
    )
    {
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
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Dictionary<string, object> startParams,
        Func<Task> action,
        [CallerMemberName] string operationName = ""
    )
    {
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
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Func<Task<T>> action,
        [CallerMemberName] string operationName = ""
    )
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (LogContext.Push(new OperationContextEnricher(context, operationName)))
        {
            T result;
            {
                logger.LogWithFields($"Starting operation {operationName}.", context);
                result = await action();
                stopwatch.Stop();
                logger.LogInformation(
                    $"Finished operation {operationName}. Elapsed: {Field.Elapsed} ms.",
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
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Func<Task<T>> action,
        [CallerMemberName] string operationName = ""
    )
    {
        if (startParams == null)
        {
            throw new ArgumentNullException(nameof(startParams));
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (LogContext.Push(new OperationContextEnricher(context, operationName)))
        {
            T result;
            {
                logger.LogWithFields($"Starting operation {operationName}.", startParams);
                result = await action();
                stopwatch.Stop();
                logger.LogInformation(
                    $"Finished operation {operationName}. Elapsed: {Field.Elapsed} ms.",
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
    public static T LogOperation<T>(
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Func<T> action,
        [CallerMemberName] string operationName = ""
    )
    {
        T result;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (LogContext.Push(new OperationContextEnricher(context, operationName)))
        {
            logger.LogWithFields($"Starting operation {operationName}.", context);
            result = action();
            stopwatch.Stop();
            logger.LogInformation(
                $"Finished operation {operationName}. Elapsed: {Field.Elapsed} ms.",
                stopwatch.ElapsedMilliseconds
            );
        }

        return result;
    }

    private static Dictionary<string, object> MakeStartMessageParams(
        Dictionary<Field, object> context
    )
    {
        return context.ToDictionary(x => x.Key.Name, x => x.Value);
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
        this Microsoft.Extensions.Logging.ILogger logger,
        OperationContext context,
        Dictionary<Field, object> startParams,
        Func<T> action,
        [CallerMemberName] string operationName = ""
    )
    {
        if (startParams == null)
        {
            throw new ArgumentNullException(nameof(startParams));
        }

        T result;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (LogContext.Push(new OperationContextEnricher(context, operationName)))
        {
            logger.LogWithFields($"Starting operation {operationName}.", startParams);
            result = action();
            stopwatch.Stop();
            logger.LogInformation(
                $"Finished operation {operationName}. Elapsed: {Field.Elapsed} ms.",
                stopwatch.ElapsedMilliseconds
            );
        }

        return result;
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
        this Microsoft.Extensions.Logging.ILogger logger,
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
}
