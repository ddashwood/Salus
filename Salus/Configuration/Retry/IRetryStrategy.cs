using Salus.Models.Entities;

namespace Salus.Configuration.Retry;

/// <summary>
/// A strategy for re-trying failed attempts at sending a message.
/// </summary>
public interface IRetryStrategy
{
    /// <summary>
    /// Gets the next time to attempt to send the message.
    /// </summary>
    /// <param name="save">
    ///     Details of the message that failed to send, including details of
    ///     when the send attempt was made, how many times it has failed, and so on.
    /// </param>
    /// <returns>The next attempt time, in UTC.</returns>
    DateTime GetNextAttemptTime(ISalusSaveEntity save);
}
