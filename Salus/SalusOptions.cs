using Salus.Configuration.Retry;

namespace Salus;

/// <summary>
/// The options to be used by Salus.
/// </summary>
public class SalusOptions
{
    /// <summary>
    /// The Action to be called to send a message synchronously.
    /// </summary>
    public Action<string>? Sender { get; private set; }
    /// <summary>
    /// The Func to be called to send a message asynchronously.
    /// </summary>
    public Func<string, Task>? SenderAsync { get; private set; }
    /// <summary>
    /// A retry strategy when a message fails to send. Currently available strategies include
    /// <see cref="ConstantRetry"/> and <see cref="ExponentialBackoffRetry"/>.
    /// </summary>
    public IRetryStrategy RetryStrategy { get; private set; } = new ConstantRetry(500);
    /// <summary>
    /// How frequently the retry queue processor runs, in milliseconds.
    /// </summary>
    public int RetryQueueProcessIntervalMilliseconds { get; private set; } = 500;

    /// <summary>
    /// Sets the Action to be called to send a message synchronously.
    /// </summary>
    /// <param name="sender">The Action to be called to send a message synchronously.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions SetMessageSender(Action<string> sender)
    {
        Sender = sender;
        return this;
    }

    /// <summary>
    /// Sets the Func to be called to send a message asynchronously.
    /// </summary>
    /// <param name="senderAsync">The Func to be called to send a message asynchronously.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions SetAsyncMessageSender(Func<string, Task> senderAsync)
    {
        SenderAsync = senderAsync;
        return this;
    }

    /// <summary>
    /// Sets the retry strategy when a message fails to send. Currently available strategies include.
    /// <see cref="ConstantRetry"/> and <see cref="ExponentialBackoffRetry"/>.
    /// </summary>
    /// <param name="retryStrategy">The retry strategy when a message fails to send.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions SetRetryStrategy(IRetryStrategy retryStrategy)
    {
        RetryStrategy = retryStrategy;
        return this;
    }

    /// <summary>
    /// Set how frequently the retry queue processor runs, in milliseconds.
    /// </summary>
    /// <param name="milliseconds">How frequently the retry queue processor should run, in milliseconds.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions SetRetryQueueProcessInterval(int milliseconds)
    {
        RetryQueueProcessIntervalMilliseconds = milliseconds;
        return this;
    }
}
