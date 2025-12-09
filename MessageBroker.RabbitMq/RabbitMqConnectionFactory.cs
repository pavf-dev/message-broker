using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.RabbitMq;

public interface IRabbitMqConnectionFactory : IAsyncDisposable
{
    /// <summary>
    /// Creates a new RabbitMQ connection
    /// </summary>
    Task<IConnection> GetConnectionAsync();
}

/// <summary>
/// Factory for creating and managing a singleton RabbitMQ connection using lazy initialization
/// </summary>
public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly RabbitMqConnectionConfiguration _configuration;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private readonly Lazy<Task<IConnection>> _lazyConnection;

    public RabbitMqConnectionFactory(
        RabbitMqConnectionConfiguration configuration,
        ILogger<RabbitMqConnectionFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _lazyConnection = new Lazy<Task<IConnection>>(
            CreateConnectionAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets or creates the RabbitMQ connection
    /// </summary>
    public Task<IConnection> GetConnectionAsync()
    {
        return _lazyConnection.Value;
    }

    /// <summary>
    /// Creates a new RabbitMQ connection
    /// </summary>
    private async Task<IConnection> CreateConnectionAsync()
    {
        _logger.LogInformation(
            "Creating RabbitMQ connection to {HostName}:{Port}",
            _configuration.HostName,
            _configuration.Port);

        var factory = new ConnectionFactory
        {
            HostName = _configuration.HostName,
            Port = _configuration.Port,
            UserName = _configuration.UserName,
            Password = _configuration.Password,
            VirtualHost = _configuration.VirtualHost,
        };

        var connection = await factory.CreateConnectionAsync();

        _logger.LogInformation(
            "Successfully connected to RabbitMQ at {HostName}:{Port}",
            _configuration.HostName,
            _configuration.Port);
        
        return connection;
    }

    /// <summary>
    /// Disposes the RabbitMQ connection
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_lazyConnection.IsValueCreated)
        {
            try
            {
                var connection = await _lazyConnection.Value;
                await connection.CloseAsync();
                connection.Dispose();
                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
        }
    }
}
