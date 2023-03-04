using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

public class ExponentialBackoffRetry : IRetryStrategy
{
    public int InitialDelayMilliseconds { get; }
    public double Multiplier { get; }
    public int? MaxDelayMilliseconds { get; }

    public ExponentialBackoffRetry(int initialDelayMilliseconds, double multiplier, int? maxDelayMilliseconds)
    {
        InitialDelayMilliseconds = initialDelayMilliseconds;
        Multiplier = multiplier;
        MaxDelayMilliseconds = maxDelayMilliseconds;
    }

    public DateTime GetNextAttemptTime(SalusSaveEntity update)
    {
        var interval = InitialDelayMilliseconds * Math.Pow(Multiplier, update.FailedMessageSendAttempts);

        if (MaxDelayMilliseconds != null && interval > MaxDelayMilliseconds)
        {
            interval = MaxDelayMilliseconds.Value;
        }

        return DateTime.UtcNow.AddMilliseconds(interval);
    }
}
