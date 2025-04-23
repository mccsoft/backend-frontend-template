using System.Threading;
using System.Threading.Tasks;

namespace MccSoft.WebHooks.Publisher;

public interface IWebHookEventPublisher
{
    /// <summary>
    /// Publishes a WebHook event by serializing the payload, storing it, and enqueuing delivery jobs.
    /// </summary>
    /// <typeparam name="T">The type of event payload.</typeparam>
    /// <param name="eventType">The type of the event being published.</param>
    /// <param name="data">The payload of the event.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishEvent<T>(string eventType, T data, CancellationToken cancellationToken = default);
};
