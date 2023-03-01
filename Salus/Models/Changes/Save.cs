using Newtonsoft.Json;
using SequentialGuid;

namespace Salus.Models.Changes;

public class Save
{
    [JsonIgnore]
    public string Id { get; }

    private List<Change> _changes;
    public IReadOnlyList<Change> Changes => _changes.AsReadOnly();

    public Save(List<Change> changes)
    {
        Id = SequentialGuidGenerator.Instance.NewGuid().ToString();
        _changes = changes;
    }
}
