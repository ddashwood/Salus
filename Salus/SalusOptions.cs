using Salus.Configuration.Retry;

namespace Salus;

/// <summary>
/// The options to be used by Salus.
/// </summary>
public class SalusOptions<TKey>
{
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
}
