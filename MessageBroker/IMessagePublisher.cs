namespace MessageBroker;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the message broker
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
}
