namespace MessageBrokerClient.Host.Bus;

public record Mars(Guid Id, DateTime Timestamp);

public record Moon(Guid Id, DateTime Timestamp);

public record Earth(Guid Id, DateTime Timestamp);
