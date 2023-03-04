namespace Salus.Exceptions;

/// <summary>
/// An exception from Salus
/// </summary>
public abstract class SalusException : ApplicationException
{
    public SalusException(string message)
        : base(message)
    {
    }
}
