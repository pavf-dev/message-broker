namespace MessageBrokerClient;

public interface IMessageHandlersRegistry
{
    IEnumerable<MessageHandlerRecord> MessageHandlers { get; }
}

public record MessageHandlerRecord(Type MessageType, Type MessageHandler);