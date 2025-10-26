using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MessageBrokerClient;

public interface IMessageDispatcher
{
    Task Dispatch(BinaryData messageBody, Type messageType);
}

public class MessageDispatcher : IMessageDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _registry;
    private readonly ILogger _logger;

    public MessageDispatcher(
        IServiceProvider serviceProvider,
        IMessageHandlersRegistry messageHandlersRegistry,
        ILogger<MessageDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _registry = messageHandlersRegistry.MessageHandlers.ToDictionary(x => x.MessageType.FullName!, x => x.MessageHandler);
        _logger = logger;
    }

    public async Task Dispatch(BinaryData messageBody, Type messageType)
    {
        if (!_registry.TryGetValue(messageType.FullName!, out var handlerType))
        {
            return;
        }
        
        try
        {
            var message = JsonSerializer.Deserialize(messageBody, messageType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var handleMethod = handlerType.GetMethod(nameof(IMessageHandler<MessageDispatcher>.Handle));
            var task = handleMethod.Invoke(handler, new[] { message });
            await (Task)task;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception in {Type}", handlerType.Name);
        }
    }
}