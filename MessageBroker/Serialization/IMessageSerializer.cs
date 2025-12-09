namespace MessageBroker.Serialization;

public interface IMessageSerializer
{
    Task<BinaryData> SerializeAsync(object message);
    
    Task<object?> DeserializeAsync(BinaryData message, Type returnType);
}
