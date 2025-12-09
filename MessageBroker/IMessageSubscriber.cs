namespace MessageBroker;

public interface IMessageSubscriber : IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);
}