using Newtonsoft.Json;
using SequentialGuid;
using System.Reflection;

namespace Salus.Models.Changes;

internal class Save
{
    [JsonIgnore]
    public string Id { get; }
    public string Version { get; }

    private List<ChangedRow> _changes;
    public IReadOnlyList<ChangedRow> Changes => _changes.AsReadOnly();

    public Save(List<ChangedRow> changes)
    {
        Id = SequentialGuidGenerator.Instance.NewGuid().ToString();
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        _changes = changes;
    }
}
