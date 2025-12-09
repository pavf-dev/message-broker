using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.DependencyInjection;

public class MessageBrokerBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}