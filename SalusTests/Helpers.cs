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
    public static SalusCore BuildTestSalus(SalusOptions? options = null)
    {
        options = options ?? new SalusOptions();

        var checker = new DbContextIdempotencyChecker();
        var saver = new DbContextSaver();
        var messageSender = new MessageSender(options, new Mock<ILogger<MessageSender>>().Object);
        return new SalusCore(checker, saver, messageSender, new SalusOptions(), new Mock<ILogger<SalusCore>>().Object);
    }

    public static string FixVersion(string json)
    {
        var version = typeof(SalusCore).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        return json.Replace("\"Version\":\"TBC\"", $"\"Version\":\"{version}\"");
    }
}
