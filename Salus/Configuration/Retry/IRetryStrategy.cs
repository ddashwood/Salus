using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

public interface IRetryStrategy
{
    DateTime GetNextAttemptTime(SalusSaveEntity update);
}
