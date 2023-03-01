using Microsoft.EntityFrameworkCore;
using Moq;
using Salus;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.Messaging;

public class MessagingTests
{
    private const string ADD_JSON = """{"Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";

    public interface IMessageSender
    {
        void Send(string message);
    }

    [Fact]
    public void MessageTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send));

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);

        Helpers.CreateDatabaseTables(context);

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = context.SaveChanges();

        // Assert

        mockSender.Verify(m => m.Send(ADD_JSON), Times.Once);
    }

    [Fact]
    public void MessageWithRollbackTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send));

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);

        Helpers.CreateDatabaseTables(context);

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
    }

    [Fact]
    public void MessageWithCommitTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send));

        var dbOptions = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, dbOptions);

        Helpers.CreateDatabaseTables(context);
        
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

        mockSender.Verify(m => m.Send(ADD_JSON), Times.Once);
    }

}
