namespace Salus.Exceptions;

public abstract class SalusException : ApplicationException
{
    public SalusException(string message)
        : base(message)
    {
    }
}
