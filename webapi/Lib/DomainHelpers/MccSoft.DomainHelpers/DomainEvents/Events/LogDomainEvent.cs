using Microsoft.Extensions.Logging;

namespace MccSoft.DomainHelpers.DomainEvents.Events
{
    /// <summary>
    /// Domain event which logs its content to standard logger on SaveChanges.
    /// </summary>
    public class LogDomainEvent : IDomainEventWithIntegerId
    {
        public static LogDomainEvent Info(string message, params object[] parameters)
        {
            return new LogDomainEvent(LogLevel.Information, message, parameters);
        }

        public static LogDomainEvent Warning(string message, params object[] parameters)
        {
            return new LogDomainEvent(LogLevel.Warning, message, parameters);
        }

        public LogDomainEvent(LogLevel level, string message, params object[] parameters)
        {
            Level = level;
            Message = message;
            Parameters = parameters;
        }

        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public object[] Parameters { get; set; }
        public int Id { get; set; }
    }
}
