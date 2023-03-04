using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

/// <summary>
/// A strategy for retrying the sending of a message at fixed intervals.
/// </summary>
public class ConstantRetry : IRetryStrategy
{
    /// <summary>
    /// The interval between retries.
    /// </summary>
    public int RetryIntervalMilleseconds { get; }

    /// <summary>
    /// Creates an instance of <see cref="ConstantRetry"/>.
    /// </summary>
    /// <param name="retryIntervalMilliseconds">The interval between retries.</param>
    public ConstantRetry(int retryIntervalMilliseconds)
    {
        RetryIntervalMilleseconds = retryIntervalMilliseconds;
    }
    
    /// <inheritdoc/>
    public DateTime GetNextAttemptTime(SalusSaveEntity save)
    {
        return DateTime.UtcNow.AddMilliseconds(RetryIntervalMilleseconds);
    }
}
