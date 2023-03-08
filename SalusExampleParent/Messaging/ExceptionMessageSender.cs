using Salus.Messaging;

namespace SalusExampleParent.Messaging;

internal class ExceptionMessageSender : IAsyncMessageSender
{
    public Task SendAsync(string message)
    {
        throw new Exception("Simulated message sending exception");
    }
}
