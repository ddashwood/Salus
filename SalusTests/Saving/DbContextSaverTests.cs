using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Salus;
using Salus.Idempotency;
using Salus.Models.Changes;
using Salus.Saving;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.Saving;

public class DbContextSaverTests
{
    private const string ADD_JSON = """{"Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";
    private const string UPDATE_JSON = """{"Changes":[{"ChangeType":1,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":[{"Name":"Name","Value":"Test Name 2"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID 2"}]}]}""";
    private const string DELETE_JSON = """{"Changes":[{"ChangeType":2,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","UpdatedFields":null,"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID 2"}]}]}""";

    [Fact]
    public void AddSaveChangesTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        // Act
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID",
            Name = "Test Name"
        });

        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(ADD_JSON, context.SalusDataChanges.Single().UpdateJson);
    }

    [Fact]
    public void UpdateSaveChangesTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 1",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 2",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 3",
            Name = "Test Name"
        });
        context.SaveChanges();
        context.Database.ExecuteSql($"DELETE FROM SalusDataChanges");

        // Act
        var entity = context.Ents.Single(e => e.Id == "Test ID 2");
        entity.Name = "Test Name 2";
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(3, context.Ents.Count());
        Assert.Equal("Test Name 2", context.Ents.Single(e => e.Id == "Test ID 2").Name);
        Assert.True(context.Ents.Where(e => e.Id != "Test ID 2").All(e => e.Name == "Test Name"));

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(UPDATE_JSON, context.SalusDataChanges.Single().UpdateJson);
    }

    [Fact]
    public void DeleteSaveChangesTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 1",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 2",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 3",
            Name = "Test Name"
        });
        context.SaveChanges();
        context.Database.ExecuteSql($"DELETE FROM SalusDataChanges");

        // Act
        var entity = context.Ents.Single(e => e.Id == "Test ID 2");
        context.Ents.Remove(entity);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(2, context.Ents.Count());
        Assert.True(context.Ents.All(e => e.Id != "Test ID 2"));

        Assert.Equal(1, context.SalusDataChanges.Count());
        Assert.Equal(DELETE_JSON, context.SalusDataChanges.Single().UpdateJson);
    }

    [Fact]
    public void AddApplyTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        // Act
        var save= JsonConvert.DeserializeObject<Save>(ADD_JSON)!;
        context.Apply(save);

        // Assert
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(0, context.SalusDataChanges.Count());
    }

    [Fact]
    public void UpdateApplyTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 1",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 2",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 3",
            Name = "Test Name"
        });
        context.SaveChanges();
        context.Database.ExecuteSql($"DELETE FROM SalusDataChanges");

        // Act
        var save = JsonConvert.DeserializeObject<Save>(UPDATE_JSON)!;
        context.Apply(save);

        // Assert
        Assert.Equal(3, context.Ents.Count());
        Assert.Equal("Test Name 2", context.Ents.Single(e => e.Id == "Test ID 2").Name);
        Assert.True(context.Ents.Where(e => e.Id != "Test ID 2").All(e => e.Name == "Test Name"));

        Assert.Equal(0, context.SalusDataChanges.Count());
    }

    [Fact]
    public void DeleteApplyTest()
    {
        // Arrange
        var checkerMock = new Mock<IDbContextIdempotencyChecker>();
        var saver = new DbContextSaver();
        var salus = new SalusCore(checkerMock.Object, saver);

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.Database.OpenConnection();
        context.Database.ExecuteSql($"CREATE TABLE Ents (Id VARCHAR(1000) PRIMARY KEY, Name VARCHAR(1000))");
        context.Database.ExecuteSql($"CREATE TABLE SalusDataChanges (Id VARCHAR(1000) PRIMARY KEY, UpdateDateTimeUtc VARCHAR(1000), UpdateJson VARCHAR(10000))");

        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 1",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 2",
            Name = "Test Name"
        });
        context.Ents.Add(new NoKeyAnnotationStringIdEntity
        {
            Id = "Test ID 3",
            Name = "Test Name"
        });
        context.SaveChanges();
        context.Database.ExecuteSql($"DELETE FROM SalusDataChanges");

        // Act
        var save = JsonConvert.DeserializeObject<Save>(DELETE_JSON)!;
        context.Apply(save);

        // Assert
        Assert.Equal(2, context.Ents.Count());
        Assert.True(context.Ents.All(e => e.Id != "Test ID 2"));

        Assert.Equal(0, context.SalusDataChanges.Count());
    }
}
