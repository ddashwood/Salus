namespace Salus;

/// <summary>
/// Apply this attribute to a DbSet to indicate that
/// Salus is to keep it updated with source data.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public  class SalusDestinationDbSetAttribute : Attribute
{
    /// <summary>
    /// The name which is used for pairing source and destination entity types with each other.
    /// If omitted, Salus will look at the class name and try to match that with the name
    /// from the source.
    /// </summary>
    public string? SalusName { get; set; }
}
