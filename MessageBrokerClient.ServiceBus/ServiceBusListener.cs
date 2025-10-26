using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageBrokerClient.ServiceBus;

public class ServiceBusListener : BackgroundService
{
    private readonly ServiceBusSubscribersConfiguration _configuration;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly ILogger<ServiceBusListener> _logger;
    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusListener(
        ServiceBusSubscribersConfiguration configuration,
        IMessageDispatcher messageDispatcher,
        ILogger<ServiceBusListener> logger)
    {
        _configuration = configuration;
        _messageDispatcher = messageDispatcher;
        _logger = logger;
        _serviceBusClient = new ServiceBusClient(configuration.ServiceBusConnectionString);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var subscriber in _configuration.Subscribers)
        {
            var processor = subscriber.ServiceBusTargetType switch 
            {
                ServiceBusTargetType.Queue => SubscribeToQueue(subscriber),
                ServiceBusTargetType.Topic => SubscribeToTopic(subscriber),
                _ => throw new ArgumentOutOfRangeException()
            };

            await processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Started processing for {TargetType}: {TargetName}", 
                subscriber.ServiceBusTargetType, subscriber.TargetName);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
    }
    
    private ServiceBusProcessor SubscribeToTopic(SubscriberConfiguration subscriber)
    {
        var processor = _serviceBusClient.CreateProcessor(subscriber.TargetName, subscriber.SubscriptionName);

        processor.ProcessMessageAsync += args => HandleMessage(args, subscriber.MessageType);
        processor.ProcessErrorAsync += ProcessorOnProcessErrorAsync;

        return processor;
    }

    private Task ProcessorOnProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Unhandled exception");

        return Task.CompletedTask;
    }

    private async Task HandleMessage(ProcessMessageEventArgs messageArgs, Type messageType)
    {
        await _messageDispatcher.Dispatch(messageArgs.Message.Body, messageType);
    }

    private ServiceBusProcessor SubscribeToQueue(SubscriberConfiguration subscriber)
    {
        var processor = _serviceBusClient.CreateProcessor(subscriber.TargetName);

        processor.ProcessMessageAsync += args => HandleMessage(args, subscriber.MessageType);
        processor.ProcessErrorAsync += ProcessorOnProcessErrorAsync;

        return processor;
    }
}