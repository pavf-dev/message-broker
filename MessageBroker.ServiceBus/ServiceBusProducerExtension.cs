using MessageBroker.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageBroker.ServiceBus;

/// <summary>
/// Extension methods for registering Azure Service Bus message producer
/// </summary>
public static class ServiceBusProducerExtension
{
    /// <summary>
    /// Adds Azure Service Bus message producer to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureProducer">Action to configure the Service Bus producer</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureProducer is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public static IServiceCollection AddServiceBusProducer(
        this IServiceCollection services, 
        Action<ServiceBusProducerConfiguration> configureProducer)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureProducer);

        var configuration = new ServiceBusProducerConfiguration();
        configureProducer(configuration);

        // Validate configuration
        if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Service Bus connection string must be configured. " +
                "Set the ConnectionString property in the configuration action.");
        }

        if (configuration.MessageEndpoints.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one message endpoint must be configured. " +
                "Use AddEndpoint<TMessage>(destination) to add message endpoints.");
        }

        // Register the configuration
        services.AddSingleton(configuration);

        // Register ServiceBusClient as a singleton
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<ServiceBusProducerConfiguration>();
            return new Azure.Messaging.ServiceBus.ServiceBusClient(config.ConnectionString);
        });

        // Register ServiceBusMessageSender as IMessageSender
        services.AddSingleton<IMessageBrokerClient>(sp =>
        {
            var serviceBusClient = sp.GetRequiredService<Azure.Messaging.ServiceBus.ServiceBusClient>();
            var config = sp.GetRequiredService<ServiceBusProducerConfiguration>();
            return new ServiceBusMessageBrokerClient(serviceBusClient, config.MessageEndpoints);
        });

        // Register IMessageEnvelopeBuilder if not already registered
        services.TryAddSingleton<IMessageSerializer, DefaultMessageSerializer>();

        // Register IMessageProducer if not already registered
        services.TryAddSingleton<IMessagePublisher, MessagePublisher>();

        return services;
    }
}
