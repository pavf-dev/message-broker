using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using MessageBroker.Serialization;

namespace MessageBroker.PerformanceTests;

// Test message class
public class TestMessage
{
    public string Content { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
}

// Test message handler
public class TestMessageHandler : IMessageHandler<TestMessage>
{
    public Task<MessageHandlingResult> Handle(TestMessage message)
    {
        return Task.FromResult(MessageHandlingResult.Success());
    }
}

// Test registry implementation
public class TestMessageHandlersRegistry : IMessageHandlersRegistry
{
    public IEnumerable<MessageHandlerRecord> MessageHandlers { get; }

    public TestMessageHandlersRegistry()
    {
        MessageHandlers = new[]
        {
            new MessageHandlerRecord(
                typeof(TestMessage),
                typeof(TestMessageHandler),
                typeof(IMessageHandler<TestMessage>))
        };
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class MessageDispatcherBenchmark
{
    private IMessageDispatcher _dispatcher = null!;
    private BinaryData _messageBody = null!;
    private Type _messageType = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IMessageSerializer, DefaultMessageSerializer>();
        services.AddSingleton<IMessageHandlersRegistry, TestMessageHandlersRegistry>();
        services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
        services.AddTransient<IMessageHandler<TestMessage>, TestMessageHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        _dispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();

        var testMessage = new TestMessage
        {
            Content = "Performance test message",
            Id = 12345,
            Timestamp = DateTime.UtcNow
        };
        
        _messageBody = BinaryData.FromObjectAsJson(testMessage);
        _messageType = typeof(TestMessage);
    }

    [Benchmark]
    public async Task<MessageHandlingResult> DispatchMessage()
    {
        return await _dispatcher.Dispatch(_messageBody, _messageType);
    }
}
