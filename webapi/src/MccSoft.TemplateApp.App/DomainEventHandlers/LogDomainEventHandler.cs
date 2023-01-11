using MccSoft.DomainHelpers.DomainEvents.Events;
using MediatR;

namespace MccSoft.TemplateApp.App.DomainEventHandlers
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

        public Task Handle(LogDomainEvent notification, CancellationToken cancellationToken) { _logger.Log(notification.Level, notification.Message, notification.Parameters);
            return Task.CompletedTask;
        }
    }
}
