﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Salus.Exceptions;
using System.Reflection;

namespace Salus.Idempotency;

internal class DbContextIdempotencyChecker : IDbContextIdempotencyChecker
{
    public void Check(ModelBuilder modelBuilder, DbContext context)
    {
        var contextType = context.GetType();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ValidateEntity(contextType, entityType);
        }
    }

    private void ValidateEntity(Type contextType, IMutableEntityType entityType)
    {
        // Find the relevant property in the context - we are only interested
        // in validating entities that are held in a SalusDbSet

        foreach (var dbContextProperty in contextType.GetProperties())
        {
            if (typeof(DbSet<>).MakeGenericType(entityType.ClrType).IsAssignableFrom(dbContextProperty.PropertyType))
            {
                if (dbContextProperty.GetCustomAttribute(typeof(SalusDbSetAttribute)) != null)
                {
                    ValidateSalusEntity(entityType);
                }

                return;
            }
        }
    }

    private static void ValidateSalusEntity(IMutableEntityType entityType)
    {
        var key = entityType.FindPrimaryKey();
        if (key == null || entityType.IsKeyless)
        {
            throw new NoKeyException(entityType.ShortName());
        }


        // We are now able to support generated values

        //foreach (var property in key.Properties)
        //{
        //    if (property.ValueGenerated != ValueGenerated.Never)
        //    {
        //        throw new AutoGeneratedKeyException(entityType.ShortName(), property.Name);
        //    }
        //}
    }
}