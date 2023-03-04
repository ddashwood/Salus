using Salus.Messaging;

namespace SalusExampleParent.Messaging;

internal class ExceptionMessageSender : IMessageSender
{
    public void Send(string message)
    {
        throw new Exception("Simulated message sending exception");
    }
}
