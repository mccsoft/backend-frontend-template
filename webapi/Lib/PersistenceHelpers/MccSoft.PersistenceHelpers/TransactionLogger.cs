using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers
{
    /// <summary>
    /// A logger to use in DbRetryHelper.
    /// Collects all the log messages written during transaction
    /// and logs them only in case if the transaction was successfully finished.
    /// </summary>
    /// <typeparam name="T">Name of the service which uses DbRetryHelper and this logger.</typeparam>
    public class TransactionLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _logEntriesCollection = new List<LogEntry>();

        private readonly ILogger _logger;

        public TransactionLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs all the collected messages.
        /// </summary>
        public void Succeed()
        {
            foreach (LogEntry entry in _logEntriesCollection)
            {
                _logger.Log(
                    entry.LogLevel,
                    entry.EventId,
                    entry.State,
                    entry.Exception,
                    (o, exception) => entry.State.ToString()
                );
            }

            _logEntriesCollection.Clear();
        }

        /// <summary>
        /// Deletes all the collected messages as unreliable.
        /// </summary>
        public void Fail()
        {
            _logEntriesCollection.Clear();
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        )
        {
            _logEntriesCollection.Add(new LogEntry(logLevel, eventId, state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

    internal class LogEntry
    {
        public LogLevel LogLevel { get; }

        public EventId EventId { get; }

        public Object State { get; }

        public Exception Exception { get; }

        public LogEntry(LogLevel logLevel, EventId eventId, Object state, Exception exception)
        {
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
        }
    }
}
