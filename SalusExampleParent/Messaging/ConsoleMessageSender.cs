using Salus.Messaging;

namespace SalusExampleParent.Messaging;

internal class ConsoleMessageSender : IMessageSender
{
    public void Send(string message)
    {
        Console.WriteLine("Sending: " + message);
    }
}
