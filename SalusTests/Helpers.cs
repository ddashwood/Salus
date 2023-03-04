using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;
using Salus;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;

namespace SalusTests;

internal static class Helpers
{
    public static SalusCore BuildTestSalus(IMessageSender? messageSender = null, IAsyncMessageSender? asyncMessageSender = null)
    {
        return BuildTestSalus(out MessageSenderInternal _, messageSender, asyncMessageSender);
    }

    public static SalusCore BuildTestSalus(out MessageSenderInternal messageSenderInternal, IMessageSender? messageSender = null, IAsyncMessageSender? asyncMessageSender = null)
    {
        var messageSenders = messageSender == null ? new List<IMessageSender>() : new List<IMessageSender> { messageSender};
        var asyncMessageSenders = asyncMessageSender == null ? new List<IAsyncMessageSender>() : new List<IAsyncMessageSender> { asyncMessageSender };

        var checker = new DbContextIdempotencyChecker();
        var saver = new DbContextSaver();
        messageSenderInternal = new MessageSenderInternal(new SalusOptions(), new Mock<ILogger<MessageSenderInternal>>().Object, messageSenders, asyncMessageSenders);
        return new SalusCore(checker, saver, messageSenderInternal, new SalusOptions(), new Mock<ILogger<SalusCore>>().Object);
    }

    public static string FixVersion(string json)
    {
        var version = typeof(SalusCore).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        return json.Replace("\"Version\":\"TBC\"", $"\"Version\":\"{version}\"");
    }
}
