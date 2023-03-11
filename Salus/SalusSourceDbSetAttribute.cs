namespace Salus;

/// <summary>
/// Apply this attribute to a DbSet to indicate
/// that it is to be observed by Salus.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public  class SalusSourceDbSetAttribute : Attribute
{
    /// <summary>
    /// The name which is used for pairing source and destination entity types with each other.
    /// If omitted, Salus will look for a destination entity type with the same class name as
    /// the source.
    /// </summary>
    public string? SalusName { get; set; }
}
