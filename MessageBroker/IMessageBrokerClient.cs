namespace MessageBroker;

public interface IMessageBrokerClient
{
    /// <summary>
    /// Publishes a message to a message broker
    /// </summary>
    /// <param name="messageType">The type of the message being published</param>
    /// <param name="message">The binary message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    // TODO: Add return type bool and corresponding logic
    Task PublishAsync(Type messageType, BinaryData message, CancellationToken cancellationToken = default);
}
