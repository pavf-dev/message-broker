using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMq.Subscriber;

/// <summary>
/// Extension methods for registering RabbitMQ message subscriber
/// </summary>
public static class RabbitMqSubscriberExtension
{
    public static RabbitMqBuilder WithSubscribers(
        this RabbitMqBuilder builder,
        Action<RabbitMqSubscriberConfiguration> configureSubscriber)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureSubscriber);

        var configuration = new RabbitMqSubscriberConfiguration();
        configureSubscriber(configuration);
        var services = builder.Services;
        
        foreach (var messageHandler in configuration.MessageHandlers)
        {
            services.AddTransient(messageHandler.MessageHandlerInterface, messageHandler.MessageHandler);
        }

        services.AddSingleton(configuration);
        services.AddSingleton<IMessageHandlersRegistry>(sp => 
            sp.GetRequiredService<RabbitMqSubscriberConfiguration>());
        services.AddSingleton<IMessageSubscriber, RabbitMqSubscriber>();

        return builder;
    }
}
