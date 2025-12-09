using RabbitMQ.Client;

const string DeadLetterExchangeName = "dlq-general";
const string DeadLetterQueueName = "dlq-general-queue";

// Connection details - Update if your RabbitMQ is not on localhost with default credentials
const string HostName = "localhost";
const int Port = 5672;
const string UserName = "name";
const string Password = "password";

Console.WriteLine("RabbitMQ Topology Creator Application");

try
{
    var factory = new ConnectionFactory()
    {
        HostName = HostName,
        Port = Port,
        UserName = UserName,
        Password = Password
    };

    using (var connection = await factory.CreateConnectionAsync())
    using (var channel = await connection.CreateChannelAsync(cancellationToken: CancellationToken.None))
    {
        await DeclareDlq(channel);
        await DeclareMars(channel);
        await DeclareEarth(channel);
        await DeclareMoon(channel);
    }
}

catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ An error occurred: {ex.Message}");
    Console.WriteLine("Please ensure your RabbitMQ server is running and accessible with the specified credentials.");
    Console.ResetColor();
}

Console.WriteLine("\nDone");
Console.ReadKey();

#region DLQ

async Task DeclareDlq(IChannel channel)
{
    await channel.ExchangeDeclareAsync(
        exchange: DeadLetterExchangeName,
        type: ExchangeType.Fanout,
        durable: true
    );

    await channel.QueueDeclareAsync(
        queue: DeadLetterQueueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    await channel.QueueBindAsync(
        queue: DeadLetterQueueName,
        exchange: DeadLetterExchangeName,
        routingKey: string.Empty
    );
}

#endregion

#region Mars Queue

async Task DeclareMars(IChannel channel)
{
    await channel.ExchangeDeclareAsync(
        exchange: "mars", 
        type: ExchangeType.Fanout, 
        durable: true
    );
    
    var mainQueueArguments = new Dictionary<string, object?>
    {
        // The essential argument to link this queue to the DLX
        { "x-dead-letter-exchange", DeadLetterExchangeName } 
        // Optional: You could also specify a custom routing key for dead-lettered messages
        // { "x-dead-letter-routing-key", "dlx-route" }
    };
    
    await channel.QueueDeclareAsync(
        queue: "mars",
        durable: true,
        exclusive: false, 
        autoDelete: false,
        arguments: mainQueueArguments
    );
    
    await channel.QueueBindAsync(
        queue: "mars", 
        exchange: "mars", 
        routingKey: string.Empty
    );
}

#endregion

#region Earth Queue

async Task DeclareEarth(IChannel channel)
{
    await channel.ExchangeDeclareAsync(
        exchange: "earth", 
        type: ExchangeType.Fanout, 
        durable: true
    );
    
    var mainQueueArguments = new Dictionary<string, object?>
    {
        // The essential argument to link this queue to the DLX
        { "x-dead-letter-exchange", DeadLetterExchangeName } 
        // Optional: You could also specify a custom routing key for dead-lettered messages
        // { "x-dead-letter-routing-key", "dlx-route" }
    };
    
    await channel.QueueDeclareAsync(
        queue: "earth",
        durable: true,
        exclusive: false, 
        autoDelete: false,
        arguments: mainQueueArguments
    );
    
    await channel.QueueBindAsync(
        queue: "earth", 
        exchange: "earth", 
        routingKey: string.Empty
    );
}

#endregion

#region Moon Queue

async Task DeclareMoon(IChannel channel)
{
    await channel.ExchangeDeclareAsync(
        exchange: "moon", 
        type: ExchangeType.Fanout, 
        durable: true
    );
    
    var mainQueueArguments = new Dictionary<string, object?>
    {
        // The essential argument to link this queue to the DLX
        { "x-dead-letter-exchange", DeadLetterExchangeName } 
        // Optional: You could also specify a custom routing key for dead-lettered messages
        // { "x-dead-letter-routing-key", "dlx-route" }
    };
    
    await channel.QueueDeclareAsync(
        queue: "moon",
        durable: true,
        exclusive: false, 
        autoDelete: false,
        arguments: mainQueueArguments
    );
    
    await channel.QueueBindAsync(
        queue: "moon", 
        exchange: "moon", 
        routingKey: string.Empty
    );
}

#endregion