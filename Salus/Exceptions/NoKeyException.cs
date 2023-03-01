namespace Salus.Exceptions;

public class NoKeyException : SalusException
{
    public NoKeyException(string typeName)
        : base($"Type {typeName} does not have a key - a key is required for data to be updated or deleted")
    {
    }
}
