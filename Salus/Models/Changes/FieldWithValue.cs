namespace Salus.Models.Changes;

public class FieldWithValue
{
    public required string Name { get; init; }
    public required object? Value { get; init; }
}
