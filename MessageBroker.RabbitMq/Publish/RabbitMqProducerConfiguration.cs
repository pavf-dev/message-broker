namespace MessageBroker.RabbitMq.Publish;

/// <summary>
/// Configuration for RabbitMQ message producer
/// </summary>
public class RabbitMqProducerConfiguration
{
    /// <summary>
    /// Gets the list of configured message endpoints
    /// </summary>
    public List<MessageEndpoint> MessageEndpoints { get; } = new();
    
    /// <summary>
    /// Adds a message endpoint configuration
    /// </summary>
    /// <typeparam name="TMessage">The type of message</typeparam>
    /// <param name="destination">The destination (exchange name) for this message type</param>
    public RabbitMqProducerConfiguration AddPublisher<TMessage>(string destination)
    {
        MessageEndpoints.Add(new MessageEndpoint(typeof(TMessage), destination));
        return this;
    }
}
