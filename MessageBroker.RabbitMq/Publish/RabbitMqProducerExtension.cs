using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMq.Publish;

public static class RabbitMqProducerExtension
{
    public static RabbitMqBuilder WithPublishers(
        this RabbitMqBuilder builder,
        Action<RabbitMqProducerConfiguration> configureProducer)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureProducer);
        
        var services = builder.Services;
        var producerConfig = new RabbitMqProducerConfiguration();
        configureProducer(producerConfig);

        if (producerConfig.MessageEndpoints.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one message endpoint must be configured. " +
                "Use AddEndpoint<TMessage>(destination) to add message endpoints.");
        }
        
        services.AddSingleton(producerConfig);
        services.AddSingleton<IMessageBrokerClient, RabbitMqMessageBrokerClient>();

        return builder;
    }
}
