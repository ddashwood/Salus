using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus.Messaging;

internal interface IMessageSenderInternal
{
    void Send(string message, SalusSaveEntity? entity, DbContext context);
    Task SendAsync(string message, SalusSaveEntity? entity, DbContext context);
}
