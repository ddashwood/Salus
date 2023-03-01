namespace Salus.Models.Changes;

internal class FieldWithValue
{
    public required string Name { get; init; }
    public required object? Value { get; init; }
}
