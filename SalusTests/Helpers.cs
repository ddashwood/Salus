using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;
using Salus;
using Moq;
using Microsoft.Extensions.Logging;
using Salus.QueueProcessing;

namespace SalusTests;

internal static class Helpers
{
    public static SalusCore<int> BuildTestSalus(IMessageSender? messageSender = null, IAsyncMessageSender? asyncMessageSender = null)
    {
        return BuildTestSalus(out MessageSenderInternal<int> _, messageSender, asyncMessageSender);
    }

    public static SalusCore<int> BuildTestSalus(out MessageSenderInternal<int> messageSenderInternal, IMessageSender? messageSender = null, IAsyncMessageSender? asyncMessageSender = null)
    {
        var messageSenders = messageSender == null ? new List<IMessageSender>() : new List<IMessageSender> { messageSender};
        var asyncMessageSenders = asyncMessageSender == null ? new List<IAsyncMessageSender>() : new List<IAsyncMessageSender> { asyncMessageSender };

        var checker = new DbContextIdempotencyChecker();
        var saver = new DbContextSaver<int>();
        messageSenderInternal = new MessageSenderInternal<int>(new SalusOptions<int>(messageSender, asyncMessageSender), new Mock<ILogger<MessageSenderInternal<int>>>().Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);
        return new SalusCore<int>(checker, saver, messageSenderInternal, new SalusOptions<int>(messageSender, asyncMessageSender), new Mock<ILogger<SalusCore<int>>>().Object, semaphoreMock.Object);
    }

    public static string FixVersion(string json)
    {
        var version = typeof(SalusCore<int>).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        return json.Replace("\"Version\":\"TBC\"", $"\"Version\":\"{version}\"");
    }
}
