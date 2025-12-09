namespace MessageBroker.RabbitMq;

/// <summary>
/// Shared configuration for RabbitMQ connections
/// </summary>
public class RabbitMqConnectionConfiguration
{
    /// <summary>
    /// Gets or sets the RabbitMQ hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the RabbitMQ port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the RabbitMQ username
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the RabbitMQ password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the RabbitMQ virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";
}
