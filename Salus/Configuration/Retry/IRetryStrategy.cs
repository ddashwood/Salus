using Salus.Models;

namespace Salus.Configuration.Retry;

public interface IRetryStrategy
{
    DateTime GetNextAttemptTime(SalusUpdateEntity update);
}
