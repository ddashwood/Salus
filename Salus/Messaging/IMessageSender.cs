using Microsoft.EntityFrameworkCore;
using Salus.Models;

namespace Salus.Messaging;

internal interface IMessageSender
{
    void Send(string message, SalusUpdateEntity? entity, DbContext context);
    Task SendAsync(string message, SalusUpdateEntity? entity, DbContext context);
}
