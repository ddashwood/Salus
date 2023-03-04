using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DatabaseGeneratedKeyImplicitContext : SalusDbContext
{
    public DatabaseGeneratedKeyImplicitContext(
        ISalus salus,
        DbContextOptions<DatabaseGeneratedKeyImplicitContext> options
    )
        : base(salus, options)
    {
    }


    [SalusDbSet]
    public DbSet<NoKeyAnnotationIntIdEntity> Ents => Set<NoKeyAnnotationIntIdEntity>();

    public void CreateDatabaseTables()
    {
        Database.OpenConnection();
        Database.ExecuteSql($"CREATE TABLE Ents (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name VARCHAR(1000))");
        Database.ExecuteSql($@"CREATE TABLE SalusSaves
                                        (Id VARCHAR(1000) NOT NULL PRIMARY KEY,
                                        UpdateDateTimeUtc VARCHAR(1000) NOT NULL,
                                        CompletedDateTimeUtc VARCHAR(1000),
                                        FailedMessageSendAttempts INT NOT NULL,
                                        LastFailedMessageSendAttemptUtc VARCHAR(1000),
                                        NextMessageSendAttemptUtc VARCHAR(1000),
                                        UpdateJson VARCHAR(10000) NOT NULL)");
    }
}
