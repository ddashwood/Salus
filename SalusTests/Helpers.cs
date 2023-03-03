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
        var messageSender = new MessageSender(options);
        return new SalusCore(checker, saver, messageSender, new SalusOptions(), new Mock<ILogger<SalusCore>>().Object);
    }
}
