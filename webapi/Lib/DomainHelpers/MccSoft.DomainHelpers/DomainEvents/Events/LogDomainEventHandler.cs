using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MccSoft.DomainHelpers.DomainEvents.Events
{
    /// <summary>
    /// Handles <see cref="LogDomainEvent"/> and logs its content to standard logger.
    /// </summary>
    public class LogDomainEventHandler : INotificationHandler<LogDomainEvent>
    {
        private readonly ILogger<LogDomainEventHandler> _logger;

        public LogDomainEventHandler(ILogger<LogDomainEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(LogDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.Log(notification.Level, notification.Message, notification.Parameters);
            return Task.CompletedTask;
        }
    }
}
