using Salus.Models;

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

    public DateTime GetNextAttemptTime(SalusUpdateEntity update)
    {
        var interval = InitialDelayMilliseconds * Math.Pow(Multiplier, update.FailedMessageSendAttempts);

        if (MaxDelayMilliseconds != null && interval > MaxDelayMilliseconds)
        {
            interval = MaxDelayMilliseconds.Value;
        }

        var x = DateTime.UtcNow.AddMilliseconds(interval);
        Console.WriteLine($"***** New time {x} with interval {interval}");
        return x;
    }
}
