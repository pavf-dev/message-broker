using Microsoft.Extensions.DependencyInjection;

namespace MessageBrokerClient;

public static class MessageBrokerClientConfiguration
{
    public static void AddMessageBrokerClient(this IServiceCollection services, IMessageHandlersRegistry messageHandlersRegistry)
    {
        services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
        services.AddSingleton<IMessageHandlersRegistry>(messageHandlersRegistry);
    }
    
}