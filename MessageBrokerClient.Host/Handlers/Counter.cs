namespace MessageBrokerClient.Host.Handlers;

public class Counter
{
    private int _receivedCount;
    private int _sentCount;
    
    public void IncrementReceived() => Interlocked.Increment(ref _receivedCount);
    
    public void IncrementSent() => Interlocked.Increment(ref _sentCount);
    
    public int ReceivedCount => _receivedCount;
    
    public int SentCount => _sentCount;
}