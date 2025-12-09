namespace MessageBroker.ServiceBus;

/// <summary>
/// Configuration for Azure Service Bus message producer
/// </summary>
public class ServiceBusProducerConfiguration
{
    private readonly List<MessageEndpoint> _messageEndpoints = new();

    /// <summary>
    /// Gets or sets the Azure Service Bus connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of message endpoints
    /// </summary>
    public IReadOnlyList<MessageEndpoint> MessageEndpoints => _messageEndpoints.AsReadOnly();

    /// <summary>
    /// Adds a message endpoint mapping a message type to a Service Bus destination
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="destination">The queue or topic name in Service Bus</param>
    /// <returns>The configuration instance for fluent chaining</returns>
    public ServiceBusProducerConfiguration AddEndpoint<TMessage>(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination cannot be null or empty", nameof(destination));

        _messageEndpoints.Add(new MessageEndpoint(typeof(TMessage), destination));
        return this;
    }

    /// <summary>
    /// Adds a message endpoint mapping a message type to a Service Bus destination
    /// </summary>
    /// <param name="messageType">The message type</param>
    /// <param name="destination">The queue or topic name in Service Bus</param>
    /// <returns>The configuration instance for fluent chaining</returns>
    public ServiceBusProducerConfiguration AddEndpoint(Type messageType, string destination)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination cannot be null or empty", nameof(destination));

        _messageEndpoints.Add(new MessageEndpoint(messageType, destination));
        return this;
    }
}
