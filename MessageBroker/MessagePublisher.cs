using MessageBroker.Serialization;

namespace MessageBroker;

public class MessagePublisher : IMessagePublisher
{
    private readonly IMessageSerializer _serializer;
    private readonly IMessageBrokerClient _messageBrokerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePublisher"/> class
    /// </summary>
    /// <param name="serializer">Message serializer. Is used to serialize a message to byte array before sending it to the message broker.</param>
    /// <param name="messageBrokerClient">The publisher that implements the work for publishing a message to the message broker.</param>
    public MessagePublisher(IMessageSerializer serializer, IMessageBrokerClient messageBrokerClient)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(messageBrokerClient);
        _serializer = serializer;
        _messageBrokerClient = messageBrokerClient;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var serializedMessage = await _serializer.SerializeAsync(message);
        await _messageBrokerClient.PublishAsync(typeof(TMessage), serializedMessage, cancellationToken);
    }
}
