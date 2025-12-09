using MessageBroker;
using MessageBrokerClient.Host.Bus;
using MessageBrokerClient.Host.Handlers;

namespace MessageBrokerClient.Host.Services;

public class MessageSenderBackgroundService : BackgroundService
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly Counter _counter;
    private readonly ILogger<MessageSenderBackgroundService> _logger;
    private readonly Random _random = new();

    public MessageSenderBackgroundService(
        IMessagePublisher messagePublisher,
        Counter counter,
        ILogger<MessageSenderBackgroundService> logger)
    {
        _messagePublisher = messagePublisher;
        _counter = counter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageSenderBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Generate random number from 3 to 5
                var threadCount = _random.Next(3, 6); // 6 is exclusive, so this gives 3, 4, or 5

                //_logger.LogInformation("Starting {ThreadCount} parallel message sending tasks", threadCount);

                // Create tasks for parallel execution
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    tasks.Add(Task.Run(async () => await SendRandomMessageAsync(stoppingToken), stoppingToken));
                }

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);

                // Wait a bit before next iteration
                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                
                _logger.LogInformation("Sent: {Sent}; received: {Received}", _counter.SentCount, _counter.ReceivedCount);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending messages");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("MessageSenderBackgroundService is stopping.");
    }

    private async Task SendRandomMessageAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Generate random message type (0, 1, or 2)
            var messageType = _random.Next(0, 0);

            var id = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;

            switch (messageType)
            {
                case 0:
                    var mars = new Mars(id, timestamp);
                    await _messagePublisher.PublishAsync(mars, cancellationToken);
                    break;
                case 1:
                    var moon = new Moon(id, timestamp);
                    await _messagePublisher.PublishAsync(moon, cancellationToken);
                    break;
                case 2:
                    var earth = new Earth(id, timestamp);
                    await _messagePublisher.PublishAsync(earth, cancellationToken);
                    break;
            }
            
            _counter.IncrementSent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
    }
}
