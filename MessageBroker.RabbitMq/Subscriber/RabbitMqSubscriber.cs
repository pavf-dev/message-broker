using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker.RabbitMq.Subscriber;

/// <summary>
/// RabbitMQ implementation of IMessageSubscriber
/// </summary>
public class RabbitMqSubscriber : IMessageSubscriber
{
    private readonly RabbitMqSubscriberConfiguration _configuration;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly ILogger<RabbitMqSubscriber> _logger;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private IChannel? _channel;
    private readonly List<string> _consumerTags = new();

    public RabbitMqSubscriber(
        RabbitMqSubscriberConfiguration configuration,
        IMessageDispatcher messageDispatcher,
        ILogger<RabbitMqSubscriber> logger,
        IRabbitMqConnectionFactory connectionFactory)
    {
        _configuration = configuration;
        _messageDispatcher = messageDispatcher;
        _logger = logger;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Starts the RabbitMQ listeners for all configured topics
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Create a check if a _channel already was initialized
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            
            foreach (var subscriber in _configuration.Subscribers)
            {
                await Subscribe(subscriber, cancellationToken);
            }

            _logger.LogInformation("Started {Count} RabbitMQ listeners", _configuration.Subscribers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ subscribers");
            throw;
        }
    }

    /// <summary>
    /// Subscribes to a topic exchange with a routing key
    /// </summary>
    private async Task Subscribe(
        QueueSubscriberConfiguration subscriber,
        CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not initialized");
        }

        await _channel.QueueDeclarePassiveAsync(subscriber.QueueName, cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += (_, eventArgs) => MessageReceivedHandlerAsync(eventArgs, subscriber);
        
        var consumerTag = await _channel.BasicConsumeAsync(
            queue: subscriber.QueueName,
            autoAck: subscriber.AutoAck,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _consumerTags.Add(consumerTag);

        _logger.LogInformation(
            "Subscribed to queue: {Queue}",
            subscriber.QueueName);
    }

    private async Task MessageReceivedHandlerAsync(BasicDeliverEventArgs eventArgs, QueueSubscriberConfiguration subscriber)
    {
        var handlingResult = await HandleMessage(eventArgs, subscriber.MessageType);

        if (subscriber.AutoAck) return;
            
        ValueTask? ackTask = handlingResult.ResultType switch
        {
            MessageHandlingResultType.Succeeded => _channel.BasicAckAsync(eventArgs.DeliveryTag, false),
            MessageHandlingResultType.FailedNoRetry => _channel.BasicRejectAsync(eventArgs.DeliveryTag, false),
            MessageHandlingResultType.FailedRetryAllowed => _channel.BasicRejectAsync(eventArgs.DeliveryTag, true),
            _ => null
        };
            
        if (ackTask is null)
        {
            // TODO: extend the error message with message type, or queue name
            _logger.LogError("Unknown message handling result type: {ResultType}", handlingResult.ResultType);

            ackTask = _channel.BasicRejectAsync(eventArgs.DeliveryTag, false);
        }

        try
        {
            await ackTask.Value;
        }
        catch (Exception e)
        {
            // TODO: improve message
            _logger.LogError(e, "Message acknowledgment failed");
        }
    }
    
    /// <summary>
    /// Handles incoming messages and dispatches them to the message dispatcher
    /// </summary>
    private async Task<MessageHandlingResult> HandleMessage(BasicDeliverEventArgs eventArgs, Type messageType)
    {
        try
        {
            var messageBody = new BinaryData(eventArgs.Body);

            _logger.LogDebug(
                "Received message from exchange: {Exchange}, routing key: {RoutingKey}",
                eventArgs.Exchange,
                eventArgs.RoutingKey);

            var result = await _messageDispatcher.Dispatch(messageBody, messageType);
            _logger.LogDebug("Message dispatched successfully");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling message from exchange: {Exchange}, routing key: {RoutingKey}",
                eventArgs.Exchange,
                eventArgs.RoutingKey);
            
            return MessageHandlingResult.FailedNoRetry();
        }
    }

    /// <summary>
    /// Disposes RabbitMQ resources (channel only, connection is managed by ConnectionFactory)
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_channel is null || _channel.IsClosed) return;
        
        try
        {
            foreach (var consumerTag in _consumerTags)
            {
                try
                {
                    await _channel.BasicCancelAsync(consumerTag);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error canceling consumer: {ConsumerTag}", consumerTag);
                }
            }

            await _channel.CloseAsync();
            _channel.Dispose();

            _logger.LogInformation("RabbitMQ subscriber disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ subscriber");
        }
    }
}
