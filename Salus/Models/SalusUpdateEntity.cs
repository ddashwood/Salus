using Newtonsoft.Json;
using Salus.Models.Changes;
using System.ComponentModel.DataAnnotations;

namespace Salus.Models;

public class SalusUpdateEntity
{
    [Key]
    public string Id { get; private set; } = string.Empty;
    public DateTime UpdateDateTimeUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedDateTimeUtc { get; set; }
    public int FailedMessageSendAttempts { get; set; }
    public DateTime? LastFailedMessageSendAttemptUtc { get; set; }
    public string UpdateJson { get; private set; } = string.Empty;

    // For Entity Framework
    private SalusUpdateEntity()
    {
    }

    internal SalusUpdateEntity(Save save)
    {
        Id = save.Id;
        UpdateDateTimeUtc = DateTime.UtcNow;
        UpdateJson = JsonConvert.SerializeObject(save);
    }
}
