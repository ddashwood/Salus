using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Salus;
using Salus.HostedServices;
using Salus.Messaging;
using Salus.Models;
using SalusTests.TestDataStructures.Contexts;

namespace SalusTests.HostedServices;

public class QueueProcessorTests
{
    [Fact]
    public async Task ProcessQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<ITestMessageSender>();
        var salusOptions = new SalusOptions()
            .SetMessageSender(senderMock.Object.Send);

        var salus = Helpers.BuildTestSalus(out MessageSender messageSender, salusOptions);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext>>>().Object, messageSender);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusDataChanges.Add(new SalusUpdateEntity("MyId", new DateTime(2020, 1, 1), null, 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(-1), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueue();

        // Assert
        senderMock.Verify(m => m.Send(json), Times.Once);
    }

    [Fact]
    public async Task ProcessCompletedQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<ITestMessageSender>();
        var salusOptions = new SalusOptions()
            .SetMessageSender(senderMock.Object.Send);

        var salus = Helpers.BuildTestSalus(out MessageSender messageSender, salusOptions);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext>>>().Object, messageSender);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusDataChanges.Add(new SalusUpdateEntity("MyId", new DateTime(2020, 1, 1), new DateTime(2020, 1, 1), 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(-1), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueue();

        // Assert
        senderMock.Verify(m => m.Send(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessFutureQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<ITestMessageSender>();
        var salusOptions = new SalusOptions()
            .SetMessageSender(senderMock.Object.Send);

        var salus = Helpers.BuildTestSalus(out MessageSender messageSender, salusOptions);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext>>>().Object, messageSender);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusDataChanges.Add(new SalusUpdateEntity("MyId", new DateTime(2020, 1, 1), null, 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(10), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueue();

        // Assert
        senderMock.Verify(m => m.Send(It.IsAny<string>()), Times.Never);
    }
}
