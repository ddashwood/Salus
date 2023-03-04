namespace Salus.Messaging;

public interface IAsyncMessageSender
{
    Task SendAsync(string message);
}
