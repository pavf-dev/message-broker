using MessageBroker.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.DependencyInjection;

public static class MessageBrokerExtensions
{
    public static MessageBrokerBuilder AddMessageBroker(this IServiceCollection services)
    {
        services.AddSingleton<IMessageSerializer, DefaultMessageSerializer>();
        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
        
        return new MessageBrokerBuilder(services);
    }
}