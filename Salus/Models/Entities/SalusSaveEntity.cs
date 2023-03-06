using Newtonsoft.Json;
using Salus.Models.Changes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Salus.Models.Entities;

/// <summary>
/// A record of data that has been saved to a database, and the sending of the message
/// to represent that save.
/// </summary>
public class SalusSaveEntity<TKey> : ISalusSaveEntity
{
    /// <summary>
    /// A unique ID for this save.
    /// </summary>
    [Key]
    public TKey Id { get; private set; } = default!;
    /// <inheritdoc/>
    public DateTime UpdateDateTimeUtc { get; private set; } = DateTime.UtcNow;
    /// <inheritdoc/>
    public DateTime? CompletedDateTimeUtc { get; internal set; }
    /// <inheritdoc/>
    public int FailedMessageSendAttempts { get; internal set; }
    /// <inheritdoc/>
    public DateTime? LastFailedMessageSendAttemptUtc { get; internal set; }
    /// <inheritdoc/>
    public DateTime? NextMessageSendAttemptUtc { get; internal set; }
    /// <inheritdoc/>
    public string SaveJson { get; private set; } = string.Empty;

    // For Entity Framework
    private SalusSaveEntity()
    {
    }

    // For testing
    internal SalusSaveEntity(TKey id, DateTime updateDateTime, DateTime? completedDataTime,
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

    internal SalusSaveEntity(Save<TKey> save)
    {
        Id = save.Id;
        UpdateDateTimeUtc = DateTime.UtcNow;
        SaveJson = JsonConvert.SerializeObject(save);
    }
}
