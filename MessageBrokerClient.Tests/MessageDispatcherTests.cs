using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace MessageBrokerClient.Tests;

public class MessageDispatcherTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IMessageHandlersRegistry> _registryMock;
    private Mock<ILogger<MessageDispatcher>> _loggerMock;
    private MessageDispatcher _client;

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _registryMock = new Mock<IMessageHandlersRegistry>();
        _loggerMock = new Mock<ILogger<MessageDispatcher>>();
        
        var messageHandlers = new List<MessageHandlerRecord>
        {
            new MessageHandlerRecord(typeof(TestMessage), typeof(TestMessageHandler))
        };
        _registryMock.Setup(r => r.MessageHandlers).Returns(messageHandlers);

        _client = new MessageDispatcher(_serviceProviderMock.Object, _registryMock.Object, _loggerMock.Object);
    }

    [Test]
    public void OnMessage_does_not_throw_error()
    {
        // Arrange
        var testMessage = new TestMessage
        {
            MessageId = "test-123",
            Timestamp = DateTime.UtcNow,
            CorrelationId = "corr-456",
            Content = "Test content"
        };

        var handler = new TestMessageHandler();
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(TestMessageHandler)))
            .Returns(handler);

        var message = new BinaryData(JsonSerializer.Serialize(testMessage));

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _client.Dispatch(message, typeof(TestMessage)));
        Assert.That(handler.MessageId, Is.EqualTo(testMessage.MessageId));
    }
    
    private class TestMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? CorrelationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    private class TestMessageHandler : IMessageHandler<TestMessage>
    {
        private TestMessage? _testMessage;

        public string MessageId => _testMessage?.MessageId ?? string.Empty;
        
        public Task Handle(TestMessage message)
        {
            _testMessage = message;
            return Task.CompletedTask;
        }
    }
}