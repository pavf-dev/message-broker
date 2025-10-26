using System.Text.Json.Serialization;

namespace MessageBrokerClient.Host;

public record DocumentMessage(
    [property: JsonPropertyName("documentId")]
    Guid DocumentId, 
    
    [property: JsonPropertyName("companyId")]
    string CompanyId, 
    
    [property: JsonPropertyName("tenantId")]
    string TenantId);

public class DocumentUpdatedMessageHandler : IMessageHandler<DocumentMessage>
{
    private readonly IServiceProvider _serviceProvider;

    public DocumentUpdatedMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public Task Handle(DocumentMessage message)
    {
        return Task.CompletedTask;
    }
}