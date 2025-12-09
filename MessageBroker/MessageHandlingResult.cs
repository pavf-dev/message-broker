namespace MessageBroker;

public class MessageHandlingResult
{
    private MessageHandlingResult()
    {
        ResultType = MessageHandlingResultType.Succeeded;
    }
    
    private MessageHandlingResult(MessageHandlingResultType resultType, string failReason)
    {
        ResultType = resultType;
        FailReason = failReason;
    }

    public bool IsSuccess => ResultType == MessageHandlingResultType.Succeeded;
    
    public MessageHandlingResultType ResultType { get; }
    
    public string FailReason { get; } = "";

    public static MessageHandlingResult Success()
    {
        return Succeeded;
    }

    public static MessageHandlingResult FailedNoRetry(string? failReason = null)
    {
        return failReason is null ? FailedNoRetryResult : new MessageHandlingResult(MessageHandlingResultType.FailedNoRetry, failReason);
    }
    
    public static MessageHandlingResult FailedRetryAllowed(string? failReason = null)
    {
        return failReason is null ? FailedRetryAllowedResult : new MessageHandlingResult(MessageHandlingResultType.FailedRetryAllowed, failReason);
    }
    
    private static readonly MessageHandlingResult Succeeded = new();
    
    private static readonly MessageHandlingResult FailedNoRetryResult = new(MessageHandlingResultType.FailedNoRetry, "");
    
    private static readonly MessageHandlingResult FailedRetryAllowedResult = new(MessageHandlingResultType.FailedRetryAllowed, "");
}


public enum MessageHandlingResultType
{
    Succeeded = 1,
    FailedNoRetry,
    FailedRetryAllowed
}