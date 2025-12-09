using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMq;

public class RabbitMqBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}