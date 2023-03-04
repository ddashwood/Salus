using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

public class ConstantRetry : IRetryStrategy
{
    public int RetryIntervalMilleseconds { get; }

    public ConstantRetry(int retryIntervalMilliseconds)
    {
        RetryIntervalMilleseconds = retryIntervalMilliseconds;
    }

    public DateTime GetNextAttemptTime(SalusSaveEntity update)
    {
        return DateTime.UtcNow.AddMilliseconds(RetryIntervalMilleseconds);
    }
}
