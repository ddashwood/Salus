using Newtonsoft.Json;
using Salus.Models.Changes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Salus.Models.Entities;

public class SalusSaveEntity
{
    [Key]
    public string Id { get; private set; } = string.Empty;
    public DateTime UpdateDateTimeUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedDateTimeUtc { get; internal set; }
    public int FailedMessageSendAttempts { get; internal set; }
    public DateTime? LastFailedMessageSendAttemptUtc { get; internal set; }
    public DateTime? NextMessageSendAttemptUtc { get; internal set; }
    public string UpdateJson { get; private set; } = string.Empty;

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
        UpdateJson = message;
    }

    internal SalusSaveEntity(Save save)
    {
        Id = save.Id;
        UpdateDateTimeUtc = DateTime.UtcNow;
        UpdateJson = JsonConvert.SerializeObject(save);
    }
}
