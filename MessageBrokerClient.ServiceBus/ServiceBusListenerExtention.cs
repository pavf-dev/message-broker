using Microsoft.Extensions.DependencyInjection;

namespace MessageBrokerClient.ServiceBus;

public static class ServiceBusListenerExtension
{
    public static void AddServiceBusListener(this IServiceCollection services, Action<ServiceBusSubscribersConfiguration> serviceBusSubscribersConfigurationBuilder)
    {
        var config = new ServiceBusSubscribersConfiguration();
        serviceBusSubscribersConfigurationBuilder(config);
        foreach (var messageHandler in config.MessageHandlers)
        {
            services.AddTransient(messageHandler.MessageHandler);
        }
        services.AddSingleton(config);
        services.AddMessageBrokerClient(config);
        services.AddHostedService<ServiceBusListener>();
    }
}