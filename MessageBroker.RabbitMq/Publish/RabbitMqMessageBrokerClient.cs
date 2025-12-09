using RabbitMQ.Client;

namespace MessageBroker.RabbitMq.Publish;

/// <summary>
/// RabbitMQ implementation of IMessageSender
/// </summary>
public sealed class RabbitMqMessageBrokerClient : IMessageBrokerClient, IAsyncDisposable
{
    private readonly Dictionary<Type, (IChannel Channel, string ExchangeName, SemaphoreSlim Semaphore)> _senders;
    private bool _disposed;
    
    public RabbitMqMessageBrokerClient(IRabbitMqConnectionFactory connectionFactory, RabbitMqProducerConfiguration producerConfig)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(producerConfig);
        
        if (!producerConfig.MessageEndpoints.Any())
            throw new ArgumentException("At least one message endpoint must be provided");

        _senders = new Dictionary<Type, (IChannel, string, SemaphoreSlim)>();
        var connection = connectionFactory.GetConnectionAsync().GetAwaiter().GetResult();

        foreach (var endpoint in producerConfig.MessageEndpoints)
        {
            if (string.IsNullOrWhiteSpace(endpoint.MessageDestination))
                throw new ArgumentException($"MessageDestination cannot be null or empty for message type {endpoint.MessageType.Name}");

            if (_senders.ContainsKey(endpoint.MessageType))
                throw new ArgumentException($"Duplicate message type found: {endpoint.MessageType.Name}");
            
            var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

            channel.ExchangeDeclarePassiveAsync(endpoint.MessageDestination).GetAwaiter().GetResult();
            
            var semaphore = new SemaphoreSlim(1, 1);
            _senders.Add(endpoint.MessageType, (channel, endpoint.MessageDestination, semaphore));
        }
    }
    
    /// <inheritdoc />
    public async Task PublishAsync(Type messageType, BinaryData message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentNullException.ThrowIfNull(message);

        if (!_senders.TryGetValue(messageType, out var publisherInfo))
        {
            throw new InvalidOperationException(
                $"No sender configured for message type '{messageType.Name}'. " +
                $"Ensure a MessageEndpoint is registered for this message type.");
        }

        var (channel, exchangeName, semaphore) = publisherInfo;
        
        // Channels of the RabbitMQ lib are not thread-safe,
        // so we have to make sure that only one thread is sending it at the moment.
        // TODO: Add timeout
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: string.Empty,
                mandatory: true,
                basicProperties: new BasicProperties(),
                body: message.ToMemory(),
                cancellationToken: cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        foreach (var (channel, _, semaphore) in _senders.Values)
        {
            channel.CloseAsync();
            channel.Dispose();
            semaphore.Dispose();
        }

        _senders.Clear();
        _disposed = true;

        return ValueTask.CompletedTask;
    }
}
