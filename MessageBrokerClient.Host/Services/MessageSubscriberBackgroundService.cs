using MessageBroker;

namespace MessageBrokerClient.Host.Services;

public class MessageSubscriberBackgroundService : BackgroundService
{
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly ILogger<MessageSubscriberBackgroundService> _logger;

    public MessageSubscriberBackgroundService(
        IMessageSubscriber messageSubscriber,
        ILogger<MessageSubscriberBackgroundService> logger)
    {
        _messageSubscriber = messageSubscriber;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageSubscriberBackgroundService is starting.");

        try
        {
            await _messageSubscriber.StartAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MessageSubscriberBackgroundService is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in MessageSubscriberBackgroundService");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MessageSubscriberBackgroundService is stopping.");
        await _messageSubscriber.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
