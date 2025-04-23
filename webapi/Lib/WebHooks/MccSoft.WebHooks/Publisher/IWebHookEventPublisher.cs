using System.Threading;
using System.Threading.Tasks;

public interface IWebHookEventPublisher
{
    Task PublishEvent<T>(string eventType, T data, CancellationToken cancellationToken = default);
};
