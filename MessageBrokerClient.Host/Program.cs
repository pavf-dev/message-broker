using MessageBrokerClient.Host;
using MessageBrokerClient.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddServiceBusListener(config => 
    config
        .WithConnectionString("")
        .AddQueue<DocumentMessage, DocumentUpdatedMessageHandler>("documents"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();