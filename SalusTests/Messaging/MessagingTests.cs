using Microsoft.EntityFrameworkCore;
using Moq;
using Salus;
using Salus.Messaging;
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
        var salus = Helpers.BuildTestSalus(mockSender.Object);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);

        context.CreateDatabaseTables();

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
        var salus = Helpers.BuildTestSalus(mockSender.Object);

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);

        context.CreateDatabaseTables();

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
}
