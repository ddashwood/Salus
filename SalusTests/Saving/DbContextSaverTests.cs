using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Salus.Models.Changes;
using SalusTests.TestDataStructures.Contexts;
using SalusTests.TestDataStructures.Entities;
using System.Diagnostics;

namespace SalusTests.Saving;

public class DbContextSaverTests
{
    private const string ADD_JSON = """{"Version":"TBC","Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","ChangeSalusType":"NoKeyAnnotationStringIdEntity","UpdatedFields":[{"Name":"Id","Value":"Test ID"},{"Name":"Name","Value":"Test Name"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID"}]}]}""";
    private const string UPDATE_JSON = """{"Version":"TBC","Changes":[{"ChangeType":1,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","ChangeSalusType":"NoKeyAnnotationStringIdEntity","UpdatedFields":[{"Name":"Name","Value":"Test Name 2"}],"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID 2"}]}]}""";
    private const string DELETE_JSON = """{"Version":"TBC","Changes":[{"ChangeType":2,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationStringIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","ChangeSalusType":"NoKeyAnnotationStringIdEntity","UpdatedFields":null,"PrimaryKeyFields":[{"Name":"Id","Value":"Test ID 2"}]}]}""";

    private const string AUTO_GENERATE_JSON = """{"Version":"TBC","Changes":[{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationIntIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","ChangeSalusType":"NoKeyAnnotationIntIdEntity","UpdatedFields":[{"Name":"Id","Value":1},{"Name":"Name","Value":"Test Name 1"}],"PrimaryKeyFields":[{"Name":"Id","Value":1}]},{"ChangeType":0,"ChangeClrType":"SalusTests.TestDataStructures.Entities.NoKeyAnnotationIntIdEntity, SalusTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","ChangeSalusType":"NoKeyAnnotationIntIdEntity","UpdatedFields":[{"Name":"Id","Value":2},{"Name":"Name","Value":"Test Name 2"}],"PrimaryKeyFields":[{"Name":"Id","Value":2}]}]}""";

    [Fact]
    public void AddSaveChangesTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

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

        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(ADD_JSON), context.SalusSaves.Single().SaveJson);
    }

    [Fact]
    public void UpdateSaveChangesTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

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
        context.Database.ExecuteSql($"DELETE FROM SalusSaves");

        // Act
        var entity = context.Ents.Single(e => e.Id == "Test ID 2");
        entity.Name = "Test Name 2";
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(3, context.Ents.Count());
        Assert.Equal("Test Name 2", context.Ents.Single(e => e.Id == "Test ID 2").Name);
        Assert.True(context.Ents.Where(e => e.Id != "Test ID 2").All(e => e.Name == "Test Name"));

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(UPDATE_JSON), context.SalusSaves.Single().SaveJson);
    }

    [Fact]
    public void DeleteSaveChangesTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

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
        context.Database.ExecuteSql($"DELETE FROM SalusSaves");

        // Act
        var entity = context.Ents.Single(e => e.Id == "Test ID 2");
        context.Ents.Remove(entity);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(2, context.Ents.Count());
        Assert.True(context.Ents.All(e => e.Id != "Test ID 2"));

        Assert.Equal(1, context.SalusSaves.Count());
        Assert.Equal(Helpers.FixVersion(DELETE_JSON), context.SalusSaves.Single().SaveJson);
    }

    [Fact]
    public void AddApplyTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

        // Act
        var save = JsonConvert.DeserializeObject<Save<int>>(ADD_JSON)!;
        context.Apply(save);

        // Assert
        Assert.Equal(1, context.Ents.Count());
        Assert.Equal("Test ID", context.Ents.Single().Id);
        Assert.Equal("Test Name", context.Ents.Single().Name);

        Assert.Equal(0, context.SalusSaves.Count());
    }

    [Fact]
    public void UpdateApplyTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

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
        context.Database.ExecuteSql($"DELETE FROM SalusSaves");

        // Act
        var save = JsonConvert.DeserializeObject<Save<int>>(Helpers.FixVersion(UPDATE_JSON))!;
        context.Apply(save);

        // Assert
        Assert.Equal(3, context.Ents.Count());
        Assert.Equal("Test Name 2", context.Ents.Single(e => e.Id == "Test ID 2").Name);
        Assert.True(context.Ents.Where(e => e.Id != "Test ID 2").All(e => e.Name == "Test Name"));

        Assert.Equal(0, context.SalusSaves.Count());
    }

    [Fact]
    public void DeleteApplyTest()
    {
        // Arrange
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<NonGeneratedKeyContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new NonGeneratedKeyContext(salus, options);

        context.CreateDatabaseTables();

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
        context.Database.ExecuteSql($"DELETE FROM SalusSaves");

        // Act
        var save = JsonConvert.DeserializeObject<Save<int>>(Helpers.FixVersion(DELETE_JSON))!;
        context.Apply(save);

        // Assert
        Assert.Equal(2, context.Ents.Count());
        Assert.True(context.Ents.All(e => e.Id != "Test ID 2"));

        Assert.Equal(0, context.SalusSaves.Count());
    }


    [Fact]
    public void AddSaveChangesAutoGeneratedTest()
    {
        var salus = Helpers.BuildTestSalus();

        var options = new DbContextOptionsBuilder<DatabaseGeneratedKeyImplicitContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new DatabaseGeneratedKeyImplicitContext(salus, options);

        context.CreateDatabaseTables();

        // Act
        context.Ents.Add(new NoKeyAnnotationIntIdEntity
        {
            Name = "Test Name 1"
        });
        context.Ents.Add(new NoKeyAnnotationIntIdEntity
        {
            Name = "Test Name 2"
        });

        var result = context.SaveChanges();

        // Assert
        Assert.Equal(2, result);
        Assert.Equal(2, context.Ents.Count());

        Assert.Equal(1, context.SalusSaves.Count());
        Debug.WriteLine(AUTO_GENERATE_JSON);
        Debug.WriteLine(context.SalusSaves.Single().SaveJson);
        Assert.Equal(Helpers.FixVersion(AUTO_GENERATE_JSON), context.SalusSaves.Single().SaveJson);
    }
}
