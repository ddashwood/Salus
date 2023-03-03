﻿using Microsoft.EntityFrameworkCore;
using Moq;
using Salus;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.Saving;

public class DbContextAsyncSaverTests
{
    private const string ADD_JSON = """{"Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";

    [Fact]
    public async Task AddSaveChangesAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

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

        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(ADD_JSON, context.SalusDataChanges.Single().UpdateJson);
        mockSender.Verify(m => m.Send(ADD_JSON), Times.Once);
    }

    [Fact]
    public async Task AddSaveChangesWithAsyncSenderAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

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

        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(ADD_JSON, context.SalusDataChanges.Single().UpdateJson);
        mockSender.Verify(m => m.Send(ADD_JSON), Times.Never);
        mockSender.Verify(m => m.SendAsync(ADD_JSON), Times.Once);
    }

    [Fact]
    public async Task AddSaveChangesWithCommitAsyncTest()
    {
        // Arrange
        var mockSender = new Mock<IMessageSender>();

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
        using (var tran = await context.Database.BeginTransactionAsync())
        {
            context.Ents.Add(new NoKeyAnnotationStringIdEntity
            {
                Id = "Test ID",
                Name = "Test Name"
            });

            result = await context.SaveChangesAsync();
            await tran.CommitAsync();
        }

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(ADD_JSON, context.SalusDataChanges.Single().UpdateJson);
        mockSender.Verify(m => m.Send(ADD_JSON), Times.Never);
        mockSender.Verify(m => m.SendAsync(ADD_JSON), Times.Once);
    }
}