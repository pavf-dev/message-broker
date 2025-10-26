namespace MessageBrokerClient;

public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task Handle(TMessage message);
}