using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Salus;
using Salus.QueueProcessing;
using Salus.Messaging;
using Salus.Models.Entities;
using SalusTests.TestDataStructures.Contexts;

namespace SalusTests.HostedServices;

public class QueueProcessorTests
{
    [Fact]
    public async Task ProcessQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<IAsyncMessageSender>();
        var salus = Helpers.BuildTestSalus(out MessageSenderInternal<int> messageSender, senderMock.Object);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext, int>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>().Object, messageSender, semaphoreMock.Object);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusSaves.Add(new SalusSaveEntity<int>(123, new DateTime(2020, 1, 1), null, 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(-1), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueueAsync();

        // Assert
        senderMock.Verify(m => m.SendAsync(json), Times.Once);
    }

    [Fact]
    public async Task ProcessCompletedQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<IAsyncMessageSender>();
        var salus = Helpers.BuildTestSalus(out MessageSenderInternal<int> messageSender, senderMock.Object);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext, int>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>().Object, messageSender, semaphoreMock.Object);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusSaves.Add(new SalusSaveEntity<int>(123, new DateTime(2020, 1, 1), new DateTime(2020, 1, 1), 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(-1), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueueAsync();

        // Assert
        senderMock.Verify(m => m.SendAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessFutureQueuedItemTest()
    {
        // Arrange
        var senderMock = new Mock<IAsyncMessageSender>();
        var salus = Helpers.BuildTestSalus(out MessageSenderInternal<int> messageSender, senderMock.Object);

        var contextOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, contextOptions);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);
        var queueProcessor = new QueueProcessor<NonGeneratedKeyContext, int>(context, new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>().Object, messageSender, semaphoreMock.Object);

        context.CreateDatabaseTables();

        var json = """{"Message":"Example}""";
        context.SalusSaves.Add(new SalusSaveEntity<int>(123, new DateTime(2020, 1, 1), null, 1,
            new DateTime(2020, 1, 1, 0, 0, 1), DateTime.Now.AddSeconds(10), json));
        context.SaveChanges();

        // Act
        await queueProcessor.ProcessQueueAsync();

        // Assert
        senderMock.Verify(m => m.SendAsync(It.IsAny<string>()), Times.Never);
    }
}
