namespace Salus.Models.Changes;

public class Save
{
    private List<Change> _changes;
    public IReadOnlyList<Change> Changes => _changes.AsReadOnly();

    public Save(List<Change> changes)
    {
        _changes = changes;
    }
}
