namespace Salus.Models.Entities
{
    public interface ISalusSaveEntity
    {
        /// <summary>
        /// The time at which the data was saved.
        /// </summary>
        DateTime UpdateDateTimeUtc { get; }
        /// <summary>
        /// The time at which the message was successfully sent.
        /// </summary>
        DateTime? CompletedDateTimeUtc { get; }
        /// <summary>
        /// The number of times the message has failed to send.
        /// </summary>
        int FailedMessageSendAttempts { get; }
        /// <summary>
        /// The time of the last failed attempt to send the message.
        /// </summary>
        DateTime? LastFailedMessageSendAttemptUtc { get;  }
        /// <summary>
        /// The next time to re-attempt sending the message.
        /// </summary>
        DateTime? NextMessageSendAttemptUtc { get; }
        /// <summary>
        /// JSON representing the saved data.
        /// </summary>
        string SaveJson { get; }

    }
}