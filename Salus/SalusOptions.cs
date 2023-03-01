using Salus.Models.Changes;

namespace Salus;

public class SalusOptions
{
    public Action<string>? Sender { get; private set; }
    public Func<string, Task>? SenderAsync { get; private set; }

    public SalusOptions SetMessageSender(Action<string> sender)
    {
        Sender = sender;
        return this;
    }

    public SalusOptions SetAsyncMessageSender(Func<string, Task> senderAsync)
    {
        SenderAsync = senderAsync;
        return this;
    }
}
