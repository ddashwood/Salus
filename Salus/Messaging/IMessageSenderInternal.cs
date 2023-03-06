using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal interface IMessageSenderInternal<TKey>
{
    void Send(string message, SalusSaveEntity<TKey>? entity, DbContext context);
    Task SendAsync(string message, SalusSaveEntity<TKey>? entity, DbContext context);
}
