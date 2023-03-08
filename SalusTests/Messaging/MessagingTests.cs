using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Salus;
using Salus.Messaging;
using Salus.Models.Entities;
using Salus.QueueProcessing;
using Salus.Services;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.Messaging;

public class MessagingTests
{
    private const string ADD_JSON = """{"Version":"TBC","Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";

    [Fact]
    public void MessageTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();
        var mockDatabaseProvider = new Mock<ISalusDbContextProvider>();
        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: options => options.SetDoNotFireAndForget(), databaseProviderMock: mockDatabaseProvider);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        IServiceScope scope = null!;
        mockDatabaseProvider.Setup(m => m.GetDatabase(context.GetType(), out scope)).Returns(context);

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = context.SaveChanges();

        // Assert
        mockSender.Verify(m => m.Send(Helpers.FixVersion(ADD_JSON)), Times.Once);
        var change = context.SalusSaves.Single();
        Assert.NotNull(change.CompletedDateTimeUtc);
        Assert.Equal(0, change.FailedMessageSendAttempts);
        Assert.Null(change.LastFailedMessageSendAttemptUtc);
    }

    [Fact]
    public void MessageWithRollbackTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();
        var salus = Helpers.BuildTestSalus(mockSender.Object);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        // Act
        using (var tran = context.Database.BeginTransaction())
        {
            context.Ents.Add(new NoKeyAnnotationStringIdEntity
            {
                Id = "Test ID",
                Name = "Test Name"
            });

            var result = context.SaveChanges();
        }

        // Assert
        mockSender.Verify(m => m.Send(It.IsAny<string>()), Times.Never);
        Assert.Equal(0, context.SalusSaves.Count());
    }

    [Fact]
    public void MessageWithCommitTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();
        var mockDatabaseProvider = new Mock<ISalusDbContextProvider>();
        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: options => options.SetDoNotFireAndForget(), databaseProviderMock: mockDatabaseProvider);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        IServiceScope scope = null!;
        mockDatabaseProvider.Setup(m => m.GetDatabase(context.GetType(), out scope)).Returns(context);

        // Act
        using (var tran = context.Database.BeginTransaction())
        {
            context.Ents.Add(new NoKeyAnnotationStringIdEntity
            {
                Id = "Test ID",
                Name = "Test Name"
            });

            var result = context.SaveChanges();

            tran.Commit();
        }

        // Assert
        mockSender.Verify(m => m.Send(Helpers.FixVersion(ADD_JSON)), Times.Once);
        Assert.NotNull(context.SalusSaves.Single().CompletedDateTimeUtc);
        var change = context.SalusSaves.Single();
        Assert.NotNull(change.CompletedDateTimeUtc);
        Assert.Equal(0, change.FailedMessageSendAttempts);
        Assert.Null(change.LastFailedMessageSendAttemptUtc);
    }

    [Fact]
    public void MessageWithFailedMessageSendTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();
        mockSender.Setup(m => m.Send(It.IsAny<string>())).Throws(new Exception());
        var mockDatabaseProvider = new Mock<ISalusDbContextProvider>();
        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: options => options.SetDoNotFireAndForget(), databaseProviderMock: mockDatabaseProvider);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        IServiceScope scope = null!;
        mockDatabaseProvider.Setup(m => m.GetDatabase(context.GetType(), out scope)).Returns(context);

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = context.SaveChanges();

        // Assert
        Assert.Null(context.SalusSaves.Single().CompletedDateTimeUtc);
        var change = context.SalusSaves.Single();
        Assert.Null(change.CompletedDateTimeUtc);
        Assert.Equal(1, change.FailedMessageSendAttempts);
        Assert.NotNull(change.LastFailedMessageSendAttemptUtc);
    }

    [Fact]
    public async void MessageWithRetryErrorTest()
    {
        // Arrange

        var mockSender = new Mock<IMessageSender>();
        var senderException = new Exception();
        mockSender.Setup(m => m.Send(It.IsAny<string>())).Throws(senderException);
        Func<SalusOptions<int>, SalusOptions<int>> salusOptions = o => o.SetErrorAfterRetries(3);

        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: salusOptions);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        context.SalusSaves.Add(new SalusSaveEntity<int>(1, DateTime.UtcNow.AddSeconds(-2), null, 3, null, DateTime.UtcNow.AddSeconds(-1), """{"Test":"Value"}"""));
        context.SaveChanges();

        var queueLoggerMock = new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>();
        var senderLoggerMock = new Mock<ILogger<MessageSenderInternal<int>>>();
        var messageSender = new MessageSenderInternal<int>(salus.Options, senderLoggerMock.Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);

        var sender = new QueueProcessor<NonGeneratedKeyContext, int>(context, queueLoggerMock.Object, messageSender, semaphoreMock.Object);

        // Act
        await sender.ProcessQueue();

        // Assert
        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == MessageSenderInternal<int>.ERROR_SENDING && @type.Name == "FormattedLogValues"),
                senderException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void MessageWithoutRetryErrorTest()
    {
        // Arrange

        var mockSender = new Mock<IMessageSender>();
        var senderException = new Exception();
        mockSender.Setup(m => m.Send(It.IsAny<string>())).Throws(senderException);
        Func<SalusOptions<int>, SalusOptions<int>> salusOptions = o => o.SetErrorAfterRetries(3);

        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: salusOptions);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        context.SalusSaves.Add(new SalusSaveEntity<int>(1, DateTime.UtcNow.AddSeconds(-2), null, 2, null, DateTime.UtcNow.AddSeconds(-1), """{"Test":"Value"}"""));
        context.SaveChanges();

        var queueLoggerMock = new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>();
        var senderLoggerMock = new Mock<ILogger<MessageSenderInternal<int>>>();
        var messageSender = new MessageSenderInternal<int>(salus.Options, senderLoggerMock.Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);

        var sender = new QueueProcessor<NonGeneratedKeyContext, int>(context, queueLoggerMock.Object, messageSender, semaphoreMock.Object);

        // Act
        await sender.ProcessQueue();

        // Assert
        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == MessageSenderInternal<int>.ERROR_SENDING && @type.Name == "FormattedLogValues"),
                senderException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void MessageWithTimerErrorTest()
    {
        // Arrange

        var mockSender = new Mock<IMessageSender>();
        var senderException = new Exception();
        mockSender.Setup(m => m.Send(It.IsAny<string>())).Throws(senderException);
        Func<SalusOptions<int>, SalusOptions<int>> salusOptions = o => o.SetErrorAfterTime(TimeSpan.FromMinutes(1));

        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: salusOptions);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        context.SalusSaves.Add(new SalusSaveEntity<int>(1, DateTime.UtcNow.AddSeconds(-70), null, 3, null, DateTime.UtcNow.AddSeconds(-1), """{"Test":"Value"}"""));
        context.SaveChanges();

        var queueLoggerMock = new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>();
        var senderLoggerMock = new Mock<ILogger<MessageSenderInternal<int>>>();
        var messageSender = new MessageSenderInternal<int>(salus.Options, senderLoggerMock.Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);

        var sender = new QueueProcessor<NonGeneratedKeyContext, int>(context, queueLoggerMock.Object, messageSender, semaphoreMock.Object);

        // Act
        await sender.ProcessQueue();

        // Assert
        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == MessageSenderInternal<int>.ERROR_SENDING && @type.Name == "FormattedLogValues"),
                senderException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void MessageWithoutTimerErrorTest()
    {
        // Arrange

        var mockSender = new Mock<IMessageSender>();
        var senderException = new Exception();
        mockSender.Setup(m => m.Send(It.IsAny<string>())).Throws(senderException);
        Func<SalusOptions<int>, SalusOptions<int>> salusOptions = o => o.SetErrorAfterTime(TimeSpan.FromMinutes(1));

        var salus = Helpers.BuildTestSalus(mockSender.Object, optionsSetter: salusOptions);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);
        context.CreateDatabaseTables();

        context.SalusSaves.Add(new SalusSaveEntity<int>(1, DateTime.UtcNow.AddSeconds(-40), null, 3, null, DateTime.UtcNow.AddSeconds(-1), """{"Test":"Value"}"""));
        context.SaveChanges();

        var queueLoggerMock = new Mock<ILogger<QueueProcessor<NonGeneratedKeyContext, int>>>();
        var senderLoggerMock = new Mock<ILogger<MessageSenderInternal<int>>>();
        var messageSender = new MessageSenderInternal<int>(salus.Options, senderLoggerMock.Object);
        var semaphoreMock = new Mock<IQueueProcessorSemaphore>();
        semaphoreMock.Setup(m => m.Start()).Returns(true);

        var sender = new QueueProcessor<NonGeneratedKeyContext, int>(context, queueLoggerMock.Object, messageSender, semaphoreMock.Object);

        // Act
        await sender.ProcessQueue();

        // Assert
        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        senderLoggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == MessageSenderInternal<int>.ERROR_SENDING && @type.Name == "FormattedLogValues"),
                senderException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
