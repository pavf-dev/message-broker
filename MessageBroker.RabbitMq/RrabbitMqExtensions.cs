using MessageBroker.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMq;

/// <summary>
/// Extension methods for configuring RabbitMQ with MessageBrokerBuilder
/// </summary>
public static class RabbitMqBuilderExtensions
{
    /// <summary>
    /// Configures RabbitMQ connection settings
    /// </summary>
    /// <param name="builder">The message broker builder</param>
    /// <param name="configureConnection">Action to configure RabbitMQ connection</param>
    /// <returns>The message broker builder for chaining</returns>
    public static RabbitMqBuilder RabbitMq(
        this MessageBrokerBuilder builder,
        Action<RabbitMqConnectionConfiguration> configureConnection)
    {
        ArgumentNullException.ThrowIfNull(configureConnection);
        
        var connectionConfig = new RabbitMqConnectionConfiguration();
        configureConnection(connectionConfig);

        if (string.IsNullOrWhiteSpace(connectionConfig.HostName))
        {
            throw new InvalidOperationException(
                "RabbitMQ hostname must be configured. " +
                "Set the HostName property in the connection configuration action.");
        }

        var services = builder.Services;
        services.AddSingleton(connectionConfig);
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        
        return new RabbitMqBuilder(builder.Services);
    }
}