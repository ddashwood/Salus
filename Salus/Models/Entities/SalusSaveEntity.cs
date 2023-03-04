using Newtonsoft.Json;
using Salus.Models.Changes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Salus.Models.Entities;

/// <summary>
/// A record of data that has been saved to a database, and the sending of the message
/// to represent that save.
/// </summary>
public class SalusSaveEntity
{
    /// <summary>
    /// A unique ID for this save.
    /// </summary>
    [Key]
    public string Id { get; private set; } = string.Empty;
    /// <summary>
    /// The time at which the data was saved.
    /// </summary>
    public DateTime UpdateDateTimeUtc { get; private set; } = DateTime.UtcNow;
    /// <summary>
    /// The time at which the message was successfully sent.
    /// </summary>
    public DateTime? CompletedDateTimeUtc { get; internal set; }
    /// <summary>
    /// The number of times the message has failed to send.
    /// </summary>
    public int FailedMessageSendAttempts { get; internal set; }
    /// <summary>
    /// The time of the last failed attempt to send the message.
    /// </summary>
    public DateTime? LastFailedMessageSendAttemptUtc { get; internal set; }
    /// <summary>
    /// The next time to re-attempt sending the message.
    /// </summary>
    public DateTime? NextMessageSendAttemptUtc { get; internal set; }
    /// <summary>
    /// JSON representing the saved data.
    /// </summary>
    public string SaveJson { get; private set; } = string.Empty;

    // For Entity Framework
    private SalusSaveEntity()
    {
    }

    // For testing
    internal SalusSaveEntity(string id, DateTime updateDateTime, DateTime? completedDataTime,
        int failedAttempts, DateTime? lastFailedSend, DateTime? nextSend, string message)
    {
        StackTrace stackTrace = new StackTrace();
        var callerAssembly = stackTrace.GetFrame(1)!.GetMethod()!.DeclaringType!.Assembly;
        if (callerAssembly.GetName().Name != "SalusTests")
        {
            throw new InvalidOperationException("Attempt to use a constructor that is designed to only be used from within tests");
        }

        Id = id;
        UpdateDateTimeUtc = updateDateTime;
        CompletedDateTimeUtc = completedDataTime;
        FailedMessageSendAttempts = failedAttempts;
        LastFailedMessageSendAttemptUtc = lastFailedSend;
        NextMessageSendAttemptUtc = nextSend;
        SaveJson = message;
    }

    internal SalusSaveEntity(Save save)
    {
        Id = save.Id;
        UpdateDateTimeUtc = DateTime.UtcNow;
        SaveJson = JsonConvert.SerializeObject(save);
    }
}
