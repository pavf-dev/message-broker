using System.Text.Json;

namespace MessageBroker.Serialization;

public class DefaultMessageSerializer : IMessageSerializer
{
    public Task<BinaryData> SerializeAsync(object message)
    {
        var binaryData = new BinaryData(message);

        return Task.FromResult(binaryData);
    }

    public Task<object?> DeserializeAsync(BinaryData message, Type returnType)
    {
        var returnObject = JsonSerializer.Deserialize(message, returnType);
        
        return Task.FromResult(returnObject);
    }
}
