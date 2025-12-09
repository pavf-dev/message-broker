namespace MessageBroker;

public record MessageEndpoint(Type MessageType, string MessageDestination);