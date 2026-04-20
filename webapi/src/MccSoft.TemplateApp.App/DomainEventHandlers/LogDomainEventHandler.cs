using MccSoft.DomainHelpers.DomainEvents.Events;
using Mediator;

namespace MccSoft.TemplateApp.App.DomainEventHandlers;

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

    public async ValueTask Handle(LogDomainEvent request, CancellationToken cancellationToken)
    {
        _logger.Log(request.Level, request.Message, request.Parameters);
    }
}
