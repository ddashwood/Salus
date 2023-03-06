using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal class MessageSenderInternal<TKey> : IMessageSenderInternal<TKey>
{
    private readonly SalusOptions<TKey> _options;
    private readonly ILogger<MessageSenderInternal<TKey>> _logger;

    public MessageSenderInternal(SalusOptions<TKey> options, ILogger<MessageSenderInternal<TKey>> logger)
    {
        _options = options;
        _logger = logger;
    }

    public void Send(string message, SalusSaveEntity<TKey>? entity, DbContext context)
    {
        try
        {
            _options.MessageSender?.Send(message);

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

    public async Task SendAsync(string message, SalusSaveEntity<TKey>? entity, DbContext context)
    {
        try
        {
            if (_options.AsyncMessageSender != null)
            {
                await _options.AsyncMessageSender.SendAsync(message).ConfigureAwait(false);
            }
            else
            {
                _options.MessageSender?.Send(message);
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
