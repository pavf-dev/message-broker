namespace MessageBroker;

public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task<MessageHandlingResult> Handle(TMessage message);
}