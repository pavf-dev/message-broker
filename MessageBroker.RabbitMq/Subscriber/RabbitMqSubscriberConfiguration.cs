namespace MessageBroker.RabbitMq.Subscriber;

/// <summary>
/// Configuration for RabbitMQ subscribers
/// </summary>
public class RabbitMqSubscriberConfiguration : IMessageHandlersRegistry
{
    private readonly List<QueueSubscriberConfiguration> _subscriberConfigurations = new();

    public IEnumerable<MessageHandlerRecord> MessageHandlers => _subscriberConfigurations;
    public IEnumerable<QueueSubscriberConfiguration> Subscribers => _subscriberConfigurations;

    /// <summary>
    /// Adds a topic subscription with a routing key
    /// </summary>
    public RabbitMqSubscriberConfiguration AddSubscriber<TMessage, TMessageHandler>(
        string queueName)
        where TMessage : class
        where TMessageHandler : IMessageHandler<TMessage>
    {
        _subscriberConfigurations.Add(
            new QueueSubscriberConfiguration(
                queueName,
                typeof(TMessage), 
                typeof(TMessageHandler),
                typeof(IMessageHandler<TMessage>)));
        
        return this;
    }
}

/// <summary>
/// Configuration for a queue subscriber
/// </summary>
public record QueueSubscriberConfiguration(
    string QueueName,
    Type MessageType,
    Type MessageHandler,
    Type MessageHandlerInterface,
    bool AutoAck = false) 
        : MessageHandlerRecord(MessageType, MessageHandler, MessageHandlerInterface);
