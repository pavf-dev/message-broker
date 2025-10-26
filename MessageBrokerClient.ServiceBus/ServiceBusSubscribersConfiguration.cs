namespace MessageBrokerClient.ServiceBus;

public class ServiceBusSubscribersConfiguration : IMessageHandlersRegistry
{
    private readonly List<SubscriberConfiguration> _subscriberConfigurations = new();

    public string ServiceBusConnectionString { get; private set; } = string.Empty;
    
    public IEnumerable<MessageHandlerRecord> MessageHandlers => _subscriberConfigurations;
    
    public IEnumerable<SubscriberConfiguration> Subscribers => _subscriberConfigurations;

    public ServiceBusSubscribersConfiguration WithConnectionString(string connectionString)
    {
        ServiceBusConnectionString = connectionString;
        
        return this;
    }
    
    public ServiceBusSubscribersConfiguration AddSubscriptions<TMessage, TMessageHandler>(string topicName, string subscriptionName)
        where TMessage : class
        where TMessageHandler : IMessageHandler<TMessage>
    {
        _subscriberConfigurations.Add(new SubscriberConfiguration(topicName, subscriptionName, typeof(TMessage), typeof(TMessageHandler), ServiceBusTargetType.Topic));
        return this;
    }

    public ServiceBusSubscribersConfiguration AddQueue<TMessage, TMessageHandler>(string topicName)
        where TMessage : class
        where TMessageHandler : IMessageHandler<TMessage>
    {
        _subscriberConfigurations.Add(new SubscriberConfiguration(topicName, null, typeof(TMessage), typeof(TMessageHandler), ServiceBusTargetType.Queue));
        return this;
    }
}

public record SubscriberConfiguration(
    string TargetName, 
    string? SubscriptionName,
    Type MessageType, 
    Type MessageHandler,
    ServiceBusTargetType ServiceBusTargetType) : MessageHandlerRecord(MessageType, MessageHandler);

public enum ServiceBusTargetType
{
    Queue = 1,
    Topic = 2
}