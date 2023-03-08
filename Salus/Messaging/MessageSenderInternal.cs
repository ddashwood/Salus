using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal class MessageSenderInternal<TKey> : IMessageSenderInternal<TKey>
{
    public const string ERROR_SENDING = "Error sending message - message will be queued to be re-tried later";
    public const string ERROR_SAVING_SUCCESS_DATA = "Error saving Salus changes";
    public const string ERROR_SAVING_FAILURE_DATA = "Error recording message send failure";

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
            // Send the message
            _options.MessageSender?.Send(message);

            // Update the database to show the message is sent
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
                _logger?.LogError(saveException, ERROR_SAVING_SUCCESS_DATA);
            }
        }
        catch (Exception ex)
        {
            // Log the failure to send the message
            if (LogError(entity))
            {
                _logger?.LogError(ex, ERROR_SENDING);
            }
            else
            {
                _logger?.LogWarning(ex, ERROR_SENDING);
            }

            // Update the database to show the message failed to send
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
                _logger?.LogError(saveException, ERROR_SAVING_FAILURE_DATA);
            }
        }


        
    }

    public async Task SendAsync(string message, SalusSaveEntity<TKey>? entity, DbContext context)
    {
        try
        {
            // Send the message
            if (_options.AsyncMessageSender != null)
            {
                await _options.AsyncMessageSender.SendAsync(message).ConfigureAwait(false);
            }
            else
            {
                _options.MessageSender?.Send(message);
            }

            // Update the database to show the message is sent
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
                _logger?.LogError(saveException, ERROR_SAVING_SUCCESS_DATA);
            }
        }
        catch (Exception ex)
        {
            // Log the failure to send the message
            if (LogError(entity))
            {
                _logger?.LogError(ex, ERROR_SENDING);
            }
            else
            {
                _logger?.LogWarning(ex, ERROR_SENDING);
            }

            // Update the database to show the message failed to send
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
                _logger?.LogError(saveException, ERROR_SAVING_FAILURE_DATA);
            }
        }
    }

    private bool LogError(SalusSaveEntity<TKey>? entity)
    {
        if (entity == null)
        {
            return false;
        }

        if (_options.ErrorAfterRetries != null && entity.FailedMessageSendAttempts >= _options.ErrorAfterRetries)
        {
            return true;
        }

        if (_options.ErrorAfterTime != null && DateTime.UtcNow - entity.UpdateDateTimeUtc >= _options.ErrorAfterTime)
        {
            return true;
        }

        return false;
    }
}
