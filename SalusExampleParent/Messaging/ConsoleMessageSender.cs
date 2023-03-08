using Salus.Messaging;

namespace SalusExampleParent.Messaging;

internal class ConsoleMessageSender : IAsyncMessageSender
{
    public Task SendAsync(string message)
    {
        Console.WriteLine("Sending: " + message);
        return Task.CompletedTask;
    }
}
