using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

/// <summary>
/// A strategy for retrying the sending of a message, with an initial interval which
/// gradually decreases.
/// </summary>
public class ExponentialBackoffRetry : IRetryStrategy
{
    /// <summary>
    /// The initial interval between retries.
    /// </summary>
    public int InitialDelayMilliseconds { get; }
    /// <summary>
    /// The amount by which the interval is multiplied on each retry.
    /// </summary>
    public double Multiplier { get; }
    /// <summary>
    /// The maximum amount of time between retries.
    /// </summary>
    public int? MaxDelayMilliseconds { get; }

    /// <summary>
    /// Creates an instance of <see cref="ExponentialBackoffRetry"/>.
    /// </summary>
    /// <param name="initialDelayMilliseconds">The initial interval between retries.</param>
    /// <param name="multiplier">The amount by which the interval is multiplied on each retry.</param>
    /// <param name="maxDelayMilliseconds">The maximum amount of time between retries.</param>
    public ExponentialBackoffRetry(int initialDelayMilliseconds, double multiplier, int? maxDelayMilliseconds)
    {
        InitialDelayMilliseconds = initialDelayMilliseconds;
        Multiplier = multiplier;
        MaxDelayMilliseconds = maxDelayMilliseconds;
    }

    /// <inheritdoc/>
    public DateTime GetNextAttemptTime(SalusSaveEntity save)
    {
        var interval = InitialDelayMilliseconds * Math.Pow(Multiplier, save.FailedMessageSendAttempts);

        if (MaxDelayMilliseconds != null && interval > MaxDelayMilliseconds)
        {
            interval = MaxDelayMilliseconds.Value;
        }

        return DateTime.UtcNow.AddMilliseconds(interval);
    }
}
