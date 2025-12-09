using MessageBroker;
using MessageBroker.DependencyInjection;
using MessageBroker.RabbitMq;
using MessageBroker.RabbitMq.Publish;
using MessageBroker.RabbitMq.Subscriber;
using MessageBrokerClient.Host.Bus;
using MessageBrokerClient.Host.Handlers;
using MessageBrokerClient.Host.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// builder.Services.AddServiceBusListener(config => 
//     config
//         .WithConnectionString("")
//         .AddQueue<DocumentMessage, DocumentUpdatedMessageHandler>("documents"));

// Register RabbitMQ producer with message endpoints

builder.Services.AddMessageBroker()
    .RabbitMq(connectionConfig =>
    {
        connectionConfig.UserName = "sa";
        connectionConfig.Password = "P@ssword";
    })
    .WithPublishers(publishersConfig =>
    {
        publishersConfig
            .AddPublisher<Mars>("mars")
            .AddPublisher<Moon>("moon")
            .AddPublisher<Earth>("earth");
    })
    .WithSubscribers(subscriberConfig =>
    {
        subscriberConfig
            .AddSubscriber<Mars, MarsMessageHandler>("mars")
            .AddSubscriber<Moon, MoonMessageHandler>("moon")
            .AddSubscriber<Earth, EarthMessageHandler>("earth");
    });

// builder.Services.AddRabbitMqProducer(
//     connectionConfig =>
//     {
//         connectionConfig.HostName = "localhost";
//         connectionConfig.Port = 5672;
//         connectionConfig.UserName = "sa";
//         connectionConfig.Password = "P@ssword";
//     },
//     producerConfig =>
//     {
//         producerConfig.AddEndpoint<Mars>("mars");
//         producerConfig.AddEndpoint<Moon>("moon");
//         producerConfig.AddEndpoint<Earth>("earth");
//     });
//
// // Register RabbitMQ subscriber with message endpoints
// builder.Services.AddRabbitMqSubscriber(
//     consumerConfig =>
//     {
//         consumerConfig.AddTopicSubscription<Mars, MarsMessageHandler>("mars", nameof(Mars));
//         consumerConfig.AddTopicSubscription<Moon, MoonMessageHandler>("moon", nameof(Moon));
//         consumerConfig.AddTopicSubscription<Earth, EarthMessageHandler>("earth", nameof(Earth));
//     });

builder.Services.AddSingleton<Counter>();
// Register BackgroundService for sending messages
builder.Services.AddHostedService<MessageSenderBackgroundService>();

// Register BackgroundService for receiving messages
builder.Services.AddHostedService<MessageSubscriberBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();