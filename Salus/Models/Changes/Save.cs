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
        // Other supported types are integer or Guid types, all of which are set in the database by default

        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        _changes = changes;
    }
}
