using Salus.Configuration.Retry;
using Salus.Messaging;
using System.Diagnostics;

namespace Salus;

/// <summary>
/// The options to be used by Salus.
/// </summary>
public class SalusOptions<TKey>
{
    internal IMessageSender? MessageSender { get; }
    internal IAsyncMessageSender? AsyncMessageSender { get; }

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
    /// After this number of retries, message sending failures will generate an error instead
    /// of a warning. If null, warnings will be generated regardless of how many retries have
    /// been attempted.
    /// </summary>
    public int? ErrorAfterRetries { get; private set; }

    /// <summary>
    /// After this amount of time since the event was created, message sending failures will
    /// generate an error instead of a warning. If null, warnings will be generated regardless
    /// of how long it has been since the event was created.
    /// </summary>
    public TimeSpan? ErrorAfterTime { get; private set; }

    // Only used for unit testing
    internal bool DoNotFireAndForget { get; private set; }




    /// <summary>
    /// Sets the retry strategy when a message fails to send. Currently available strategies include.
    /// <see cref="ConstantRetry"/> and <see cref="ExponentialBackoffRetry"/>.
    /// </summary>
    /// <param name="retryStrategy">The retry strategy when a message fails to send.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions<TKey> SetRetryStrategy(IRetryStrategy retryStrategy)
    {
        RetryStrategy = retryStrategy;
        return this;
    }


    /// <summary>
    /// Set how frequently the retry queue processor runs, in milliseconds.
    /// </summary>
    /// <param name="milliseconds">How frequently the retry queue processor should run, in milliseconds.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions<TKey> SetRetryQueueProcessInterval(int milliseconds)
    {
        RetryQueueProcessIntervalMilliseconds = milliseconds;
        return this;
    }

    /// <summary>
    /// Set after how many retries message sending failures will generate an error instead
    /// of a warning. If not called, warnings will be generated regardless of how many
    /// retries have been attempted.
    /// </summary>
    /// <param name="retries">The number of retries after which to generate an error.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions<TKey> SetErrorAfterRetries(int retries)
    {
        ErrorAfterRetries = retries;
        return this;
    }

    /// <summary>
    /// Sets after how much time message sending failures will generate an error instead
    /// of a warning. If not called, warnings will be generated regardless of how long has
    /// passed since the event was created.
    /// </summary>
    /// <param name="time">The time after which to generate an error.</param>
    /// <returns>The <see cref="SalusOptions"/ instance.></returns>
    public SalusOptions<TKey> SetErrorAfterTime(TimeSpan time)
    {
        ErrorAfterTime = time;
        return this;
    }


    internal SalusOptions<TKey> SetDoNotFireAndForget()
    {
        StackTrace stackTrace = new StackTrace();
        var callerAssembly = stackTrace.GetFrame(1)!.GetMethod()!.DeclaringType!.Assembly;
        if (callerAssembly.GetName().Name != "SalusTests")
        {
            throw new InvalidOperationException("Attempt to use the DoNotFireAndForget option from outside of unit tests");
        }

        DoNotFireAndForget = true;
        return this;
    }

    

    internal SalusOptions(IMessageSender? messageSender, IAsyncMessageSender? asyncMessageSender)
    {
        MessageSender = messageSender;
        AsyncMessageSender = asyncMessageSender;
    }
}
