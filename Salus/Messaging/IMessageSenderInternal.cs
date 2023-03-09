using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal interface IMessageSenderInternal<TKey>
{
    Task<bool> SendAsync(string message, SalusSaveEntity<TKey>? entity, DbContext context);
}
