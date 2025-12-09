
using MessageBroker;
using MessageBrokerClient.Host.Bus;

namespace MessageBrokerClient.Host.Handlers;

public class MarsMessageHandler : IMessageHandler<Mars>
{
    private readonly Counter _counter;

    public MarsMessageHandler(Counter counter)
    {
        _counter = counter;
    }

    public Task<MessageHandlingResult> Handle(Mars message)
    {
        _counter.IncrementReceived();
        return Task.FromResult(MessageHandlingResult.FailedNoRetry());
    }
}
