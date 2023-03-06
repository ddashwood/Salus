using Newtonsoft.Json;
using SequentialGuid;
using System.Reflection;

namespace Salus.Models.Changes;

internal class Save<TKey>
{
    [JsonIgnore]
    public TKey Id { get; internal set; } = default!;
    public string Version { get; }

    private List<ChangedRow> _changes;
    public IReadOnlyList<ChangedRow> Changes => _changes.AsReadOnly();

    public Save(List<ChangedRow> changes)
    {
        if (typeof(TKey) == typeof(string))
        {
            var id = SequentialGuidGenerator.Instance.NewGuid().ToString();
            GetType().GetProperty(nameof(Id))!.SetValue(this, id);
        }
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        _changes = changes;
    }
}
