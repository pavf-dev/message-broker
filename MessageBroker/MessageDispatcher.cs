using MessageBroker.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker;

public interface IMessageDispatcher
{
    Task<MessageHandlingResult> Dispatch(BinaryData messageBody, Type messageType);
}

public class MessageDispatcher : IMessageDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageSerializer _messageSerializer;
    private readonly Dictionary<string, Type> _registry;
  
    public MessageDispatcher(
        IServiceProvider serviceProvider,
        IMessageHandlersRegistry messageHandlersRegistry,
        IMessageSerializer messageSerializer)
    {
        _serviceProvider = serviceProvider;
        _messageSerializer = messageSerializer;
        _registry = messageHandlersRegistry.MessageHandlers.ToDictionary(x => x.MessageType.FullName!, x => x.MessageHandlerInterface);
    }

    public async Task<MessageHandlingResult> Dispatch(BinaryData messageBody, Type messageType)
    {
        if (!_registry.TryGetValue(messageType.FullName!, out var handlerType))
        {
            return MessageHandlingResult.FailedNoRetry();
        }
        
        var message = await _messageSerializer.DeserializeAsync(messageBody, messageType);

        if (message is null)
        {
            return MessageHandlingResult.FailedNoRetry();
        }
        
        // TODO: implement checks for corner cases (null, etc)
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IMessageHandler<MessageDispatcher>.Handle));
        var task = handleMethod.Invoke(handler, [message]);
        return await (Task<MessageHandlingResult>)task;
    }
}