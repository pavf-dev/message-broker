using Azure.Messaging.ServiceBus;

namespace MessageBroker.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of IMessageSender
/// </summary>
public sealed class ServiceBusMessageBrokerClient : IMessageBrokerClient, IAsyncDisposable
{
    private readonly Dictionary<Type, ServiceBusSender> _senders;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of ServiceBusMessageSender
    /// </summary>
    /// <param name="serviceBusClient">Azure Service Bus client instance</param>
    /// <param name="messageEndpoints">List of message endpoints defining message types and their destinations</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceBusClient or messageEndpoints is null</exception>
    /// <exception cref="ArgumentException">Thrown when messageEndpoints is empty</exception>
    public ServiceBusMessageBrokerClient(ServiceBusClient serviceBusClient, IEnumerable<MessageEndpoint> messageEndpoints)
    {
        ArgumentNullException.ThrowIfNull(serviceBusClient);
        ArgumentNullException.ThrowIfNull(messageEndpoints);

        var endpointsList = messageEndpoints.ToList();
        if (endpointsList.Count == 0)
            throw new ArgumentException("At least one message endpoint must be provided", nameof(messageEndpoints));

        _senders = new Dictionary<Type, ServiceBusSender>();

        // Create a sender for each message endpoint
        foreach (var endpoint in endpointsList)
        {
            if (string.IsNullOrWhiteSpace(endpoint.MessageDestination))
                throw new ArgumentException($"MessageDestination cannot be null or empty for message type {endpoint.MessageType.Name}");

            if (_senders.ContainsKey(endpoint.MessageType))
                throw new ArgumentException($"Duplicate message type found: {endpoint.MessageType.Name}");

            var sender = serviceBusClient.CreateSender(endpoint.MessageDestination);
            _senders.Add(endpoint.MessageType, sender);
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(Type messageType, BinaryData message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentNullException.ThrowIfNull(message);

        if (!_senders.TryGetValue(messageType, out var sender))
        {
            throw new InvalidOperationException(
                $"No sender configured for message type '{messageType.Name}'. " +
                $"Ensure a MessageEndpoint is registered for this message type.");
        }

        var serviceBusMessage = new ServiceBusMessage(message)
        {
            ContentType = "application/json",
            Subject = messageType.Name
        };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }

        _senders.Clear();
        _disposed = true;
    }
}
