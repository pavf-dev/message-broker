using MessageBroker;
using MessageBrokerClient.Host.Bus;

namespace MessageBrokerClient.Host.Handlers;

public class MoonMessageHandler : IMessageHandler<Moon>
{
    private readonly Counter _counter;

    public MoonMessageHandler(Counter counter)
    {
        _counter = counter;
    }

    public Task<MessageHandlingResult> Handle(Moon message)
    {
        _counter.IncrementReceived();
        return Task.FromResult(MessageHandlingResult.FailedNoRetry());
    }
}
