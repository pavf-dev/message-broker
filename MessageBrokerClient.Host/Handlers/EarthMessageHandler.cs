using MessageBroker;
using MessageBrokerClient.Host.Bus;

namespace MessageBrokerClient.Host.Handlers;

public class EarthMessageHandler : IMessageHandler<Earth>
{
    private readonly Counter _counter;

    public EarthMessageHandler(Counter counter)
    {
        _counter = counter;
    }

    public Task<MessageHandlingResult> Handle(Earth message)
    {
        _counter.IncrementReceived();
        return Task.FromResult(MessageHandlingResult.FailedNoRetry());
    }
}
