﻿using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class NoKeyContext : SalusDbContext
{
    public NoKeyContext(
        ISalus<int> salus,
        DbContextOptions<NoKeyContext> options
    )
        : base(salus, options)
    {
    }


    [SalusSourceDbSet]
    public DbSet<NoKeyAnnotationIntIdEntity> Ents => Set<NoKeyAnnotationIntIdEntity>();


    protected override void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoKeyAnnotationIntIdEntity>()
            .HasNoKey();
    }
}
