using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DestNonGeneratedKeyContextWithNamedSet : SalusDbContext
{
    public DestNonGeneratedKeyContextWithNamedSet(
        ISalus<int> salus,
        DbContextOptions<DestNonGeneratedKeyContextWithNamedSet> options
    )
        : base(salus, options)
    {
    }


    [SalusDestinationDbSet(SalusName = "CustomName")]
    public DbSet<NoKeyAnnotationStringIdEntity> Entities => Set<NoKeyAnnotationStringIdEntity>();

    public void CreateDatabaseTables()
    {
        Database.OpenConnection();
        Database.ExecuteSql($"CREATE TABLE Entities (Id VARCHAR(1000) NOT NULL PRIMARY KEY, Name VARCHAR(1000))");
        Database.ExecuteSql($@"CREATE TABLE SalusSaves
                                        (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        UpdateDateTimeUtc VARCHAR(1000) NOT NULL,
                                        CompletedDateTimeUtc VARCHAR(1000),
                                        FailedMessageSendAttempts INT NOT NULL,
                                        LastFailedMessageSendAttemptUtc VARCHAR(1000),
                                        NextMessageSendAttemptUtc VARCHAR(1000),
                                        SaveJson VARCHAR(10000) NOT NULL)");
    }
}
