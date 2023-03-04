using Microsoft.EntityFrameworkCore;
using Moq;
using Salus;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.Saving;

public class DbContextAsyncSaverTests
{
    private const string ADD_JSON = """{"Version":"TBC","Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";

    [Fact]
    public async Task AddSaveChangesAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<ITestMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send));

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = await context.SaveChangesAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(ADD_JSON), context.SalusSaves.Single().SaveJson);
        mockSender.Verify(m => m.Send(Helpers.FixVersion(ADD_JSON)), Times.Once);
    }

    [Fact]
    public async Task AddSaveChangesWithAsyncSenderAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<ITestMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send)
            .SetAsyncMessageSender(mockSender.Object.SendAsync));

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = await context.SaveChangesAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(ADD_JSON), context.SalusSaves.Single().SaveJson);
        mockSender.Verify(m => m.Send(Helpers.FixVersion(ADD_JSON)), Times.Never);
        mockSender.Verify(m => m.SendAsync(Helpers.FixVersion(ADD_JSON)), Times.Once);
    }

    [Fact]
    public async Task AddSaveChangesWithCommitAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<ITestMessageSender>();

        var salus = Helpers.BuildTestSalus(new SalusOptions()
            .SetMessageSender(mockSender.Object.Send)
            .SetAsyncMessageSender(mockSender.Object.SendAsync));

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();


        // Act
        int result;
        using (var tran = await context.Database.BeginTransactionAsync().ConfigureAwait(false))
        {
            context.Ents.Add(new NoKeyAnnotationStringIdEntity
            {
                Id = "Test ID",
                Name = "Test Name"
            });

            result = await context.SaveChangesAsync().ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
        }

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(ADD_JSON), context.SalusSaves.Single().SaveJson);
        mockSender.Verify(m => m.Send(Helpers.FixVersion(ADD_JSON)), Times.Never);
        mockSender.Verify(m => m.SendAsync(Helpers.FixVersion(ADD_JSON)), Times.Once);
    }
}
