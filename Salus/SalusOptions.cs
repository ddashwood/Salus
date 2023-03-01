using Salus.Models.Changes;

namespace Salus;

public class SalusOptions
{
    public Action<Save>? Sender { get; private set; }
    public Func<Save, Task>? SenderAsync { get; private set; }

    public void SetMessageSender(Action<Save> sender)
    {
        Sender = sender;
    }

    public void SetAsyncMessageSender(Func<Save, Task> senderAsync)
    {
        SenderAsync = senderAsync;
    }
}
