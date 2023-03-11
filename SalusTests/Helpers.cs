using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;
using Salus;
using Moq;
using Microsoft.Extensions.Logging;
using Salus.QueueProcessing;
using Salus.Services;

namespace SalusTests;

internal static class Helpers
{
    public static SalusCore<int> BuildTestSalus(IAsyncMessageSender? asyncMessageSender = null,
        Func<SalusOptions<int>, SalusOptions<int>>? optionsSetter = null, Mock<ISalusDbContextProvider>? databaseProviderMock = null)
    {
        return BuildTestSalus(out MessageSenderInternal<int> _, asyncMessageSender, optionsSetter, databaseProviderMock);
    }

    public static SalusCore<int> BuildTestSalus(out MessageSenderInternal<int> messageSenderInternal, IAsyncMessageSender? asyncMessageSender = null,
        Func<SalusOptions<int>, SalusOptions<int>>? optionsSetter = null, Mock<ISalusDbContextProvider>? databaseProviderMock = null)
    {
        var options = new SalusOptions<int>(asyncMessageSender);
        if (optionsSetter != null)
        {
            options = optionsSetter(options);
        }

        var checker = new DbContextIdempotencyChecker();
        var saver = new DbContextSaver<int>(new Mock<ILogger<DbContextSaver<int>>>().Object);
        messageSenderInternal = new MessageSenderInternal<int>(options, new Mock<ILogger<MessageSenderInternal<int>>>().Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);
        databaseProviderMock ??= new Mock<ISalusDbContextProvider>();
        var dbProvider = databaseProviderMock.Object;

        return new SalusCore<int>(checker, saver, messageSenderInternal, options, new Mock<ILogger<SalusCore<int>>>().Object,
            semaphoreMock.Object, dbProvider);
    }

    public static string FixVersion(string json)
    {
        var version = typeof(SalusCore<int>).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        return json.Replace("\"Version\":\"TBC\"", $"\"Version\":\"{version}\"");
    }
}
