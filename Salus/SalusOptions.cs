using Salus.Configuration.Retry;

namespace Salus;

public class SalusOptions
{
    public Action<string>? Sender { get; private set; }
    public Func<string, Task>? SenderAsync { get; private set; }
    public IRetryStrategy RetryStrategy { get; private set; } = new ConstantRetry(500);
    public int RetryQueueProcessIntervalMilliseconds { get; private set; } = 500;


    public SalusOptions SetMessageSender(Action<string> sender)
    {
        Sender = sender;
        return this;
    }

    public SalusOptions SetAsyncMessageSender(Func<string, Task> senderAsync)
    {
        SenderAsync = senderAsync;
        return this;
    }

    public SalusOptions SetRetryStrategy(IRetryStrategy retryStrategy)
    {
        RetryStrategy = retryStrategy;
        return this;
    }

    public SalusOptions SetRetryQueueProcessInterval(int milliseconds)
    {
        RetryQueueProcessIntervalMilliseconds = milliseconds;
        return this;
    }
}
