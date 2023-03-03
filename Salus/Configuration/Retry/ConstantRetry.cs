using Salus.Models;

namespace Salus.Configuration.Retry;

public class ConstantRetry : IRetryStrategy
{
    public int RetryIntervalMilleseconds { get; }

    public ConstantRetry(int retryIntervalMilliseconds)
    {
        RetryIntervalMilleseconds = retryIntervalMilliseconds;
    }

    public DateTime GetNextAttemptTime(SalusUpdateEntity update)
    {
        return DateTime.UtcNow.AddMilliseconds(RetryIntervalMilleseconds);
    }
}
