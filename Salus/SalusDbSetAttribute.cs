namespace Salus;

/// <summary>
/// Apply this attribute to a DbSet to indicate
/// that it is to be observed by Salus.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public  class SalusDbSetAttribute : Attribute
{
}
