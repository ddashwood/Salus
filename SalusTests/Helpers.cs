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

    public static void CreateDatabaseTables(DbContext context)
    {
        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) NOT NULL PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($@"CREATE TABLE SalusDataChanges
                                        (Id VARCHAR(1000) NOT NULL PRIMARY KEY,
                                        UpdateDateTimeUtc VARCHAR(1000) NOT NULL,
                                        CompletedDateTimeUtc VARCHAR(1000),
                                        FailedMessageSendAttempts INT NOT NULL,
                                        LastFailedMessageSendAttemptUtc VARCHAR(1000),
                                        UpdateJson VARCHAR(10000) NOT NULL)");
    }
}
