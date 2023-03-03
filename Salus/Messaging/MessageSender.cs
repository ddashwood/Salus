namespace Salus.Messaging;

internal class MessageSender : IMessageSender
{
    private readonly SalusOptions _options;

    public MessageSender(SalusOptions options)
    {
        _options = options;
    }

    public void Send(string message)
    {
        _options.Sender?.Invoke(message);
    }

    public async Task SendAsync(string message)
    {
        if (_options.SenderAsync != null)
        {
            await _options.SenderAsync(message);
            return;
        }
        _options.Sender?.Invoke(message);
    }
}
