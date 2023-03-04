using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal class MessageSenderInternal : IMessageSenderInternal
{
    private readonly SalusOptions _options;
    private readonly ILogger<MessageSenderInternal> _logger;
    private readonly IEnumerable<IMessageSender> _messageSenders;
    private readonly IEnumerable<IAsyncMessageSender> _asyncMessageSenders;

    public MessageSenderInternal(SalusOptions options, ILogger<MessageSenderInternal> logger, IEnumerable<IMessageSender> messageSenders, IEnumerable<IAsyncMessageSender> asyncMessageSenders)
    {
        _options = options;
        _logger = logger;
        _messageSenders = messageSenders;
        _asyncMessageSenders = asyncMessageSenders;
    }

    public void Send(string message, SalusSaveEntity? entity, DbContext context)
    {
        try
        {
            foreach (var sender in _messageSenders)
            {
                sender.Send(message);
            }

            try
            {
                if (entity != null)
                {
                    entity.CompletedDateTimeUtc = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error saving Salus changes");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error sending message - message will be queued to be re-tried later");

            try
            {
                if (entity != null)
                {
                    entity.LastFailedMessageSendAttemptUtc = DateTime.UtcNow;
                    entity.NextMessageSendAttemptUtc = _options.RetryStrategy.GetNextAttemptTime(entity);
                    entity.FailedMessageSendAttempts++;
                    context.SaveChanges();
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error recording message send failure");
            }
        }


        
    }

    public async Task SendAsync(string message, SalusSaveEntity? entity, DbContext context)
    {
        try
        {
            if (_asyncMessageSenders.Any())
            {
                foreach (var sender in _asyncMessageSenders)
                {
                    await sender.SendAsync(message).ConfigureAwait(false);
                }
            }
            else
            {
                foreach (var sender in _messageSenders)
                {
                    sender.Send(message);
                }
            }

            try
            {
                if (entity != null)
                {
                    entity.CompletedDateTimeUtc = DateTime.UtcNow;
                    await context.SaveChangesAsync().ConfigureAwait(false); ;
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error saving Salus changes");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error sending message - message will be queued to be re-tried later");

            try
            {
                if (entity != null)
                {
                    entity.LastFailedMessageSendAttemptUtc = DateTime.UtcNow;
                    entity.NextMessageSendAttemptUtc = _options.RetryStrategy.GetNextAttemptTime(entity);
                    entity.FailedMessageSendAttempts++;
                    await context.SaveChangesAsync().ConfigureAwait(false); ;
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error recording message send failure");
            }
        }
    }
}
