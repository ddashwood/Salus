namespace Salus.Exceptions;

/// <summary>
/// The exception that is thrown when no key exists and this is not allowed.
/// </summary>
public class NoKeyException : SalusException
{
    public NoKeyException(string typeName)
        : base($"Type {typeName} does not have a key - a key is required for data to be updated or deleted")
    {
    }
}
