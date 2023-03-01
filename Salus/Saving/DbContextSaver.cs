using Microsoft.EntityFrameworkCore;
using Salus.Models;
using Salus.Models.Changes;
using System.Linq.Expressions;
using System.Reflection;

namespace Salus.Saving;

public class DbContextSaver : IDbContextSaver
{
    // When applying changes from a collection of Change objects, we need to ensure that we
    // don't add those changes into the SalusDataChanges table in the database - the changes have
    // come from elsewhere in the system, and the Updates table only references the initial
    // change of the data, not when it is updated elsewhere
    private bool _applying;

    public Save? SaveChanges(DbContext context)
    {
        if (_applying)
        {
            return null;
        }

        context.ChangeTracker.DetectChanges();

        List<Change> changes = new();

        var changeTrackerEntries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in changeTrackerEntries)
        {
            Change? change = null;

            switch (entry.State)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    change = new Change(entry);
                    break;
            }

            if (change != null)
            {
                changes.Add(change);
            }
        }


        // TO DO - handle messaging and resilliance

        // If *not* in a transaction, then SaveChanges acts as a transaction
        // In this case, we can SaveChanges, then publish to the message system.
        // After that, we can update SalusDataChanges to show the change is
        // complete.
        //
        // If we *are* in a transaction, we can't publish the message yet.
        // We should probably do that when the transaction has been commited.

        return new Save(changes);
    }

    public Task<Save> SaveChangesAsync(CancellationToken cancellationToken, DbContext context)
    {
        throw new NotImplementedException();
    }

    public void Apply(DbContext context, IEnumerable<Change> changes)
    {
        try
        {
            _applying = true;

            foreach (var change in changes)
            {
                switch (change.ChangeType)
                {
                    case Change.ChangeTypeEnum.Insert:
                        ApplyInsert(context, change);
                        break;
                    case Change.ChangeTypeEnum.Update:
                        ApplyUpdate(context, change);
                        break;
                    case Change.ChangeTypeEnum.Delete:
                        ApplyDelete(context, change);
                        break;
                    default:
                        break;
                }
            }

            context.SaveChanges(true);
        }
        finally
        {
            _applying = false;
        }
    }

    private void ApplyInsert(DbContext context, Change change)
    {
        var existingEntity = GetEntityFromDatabaseWithPrimaryKey(context, change, out var entityType);
        if (existingEntity != null)
        {
            return; // This entity already exists in the database
        }

        // Create and populate the entity
        var entity = Activator.CreateInstance(entityType);
        ApplyFieldChanges(change, entityType, entity);

        // Add the entity to the database
        var dbSetPropertyInfo = context
            .GetType()
            .GetProperties()
            .Single(p => typeof(DbSet<>)
                            .MakeGenericType(entityType)
                            .IsAssignableFrom(p.PropertyType));
        var dbSet = dbSetPropertyInfo.GetGetMethod()!.Invoke(context, null);
        dbSetPropertyInfo.PropertyType.GetMethod("Add")!.Invoke(dbSet, new object?[] { entity });
    }

    private void ApplyUpdate(DbContext context, Change change)
    {
        var entity = GetEntityFromDatabaseWithPrimaryKey(context, change, out var entityType);
        ApplyFieldChanges(change, entityType, entity);
    }

    private void ApplyDelete(DbContext context, Change change)
    {
        var entity = GetEntityFromDatabaseWithPrimaryKey(context, change, out var entityType);
        if (entity == null)
        {
            return; // The entity has already been deleted
        }

        // Find the DbSet and delete the entity
        var dbSetPropertyInfo = context
            .GetType()
            .GetProperties()
            .Single(p => typeof(DbSet<>)
                            .MakeGenericType(entityType)
                            .IsAssignableFrom(p.PropertyType));
        var dbSet = dbSetPropertyInfo.GetGetMethod()!.Invoke(context, null);
        dbSetPropertyInfo.PropertyType.GetMethod("Remove")!.Invoke(dbSet, new object?[] { entity });
    }

    private static object? GetEntityFromDatabaseWithPrimaryKey(DbContext context, Change change, out Type entityType)
    {
        // N.b. need to create a local copy of this, because we are not allowed to use "out" parameters in
        // lambdas such as when we get the dbSetPropertyInfo below
        var localEntityType = entityType = Type.GetType(change.ChangeClrType)
            ?? throw new InvalidOperationException("Can't find type when applying change (insert): " + change.ChangeClrType);

        var dbSetPropertyInfo = context
            .GetType()
            .GetProperties()
            .Single(p => typeof(DbSet<>)
                            .MakeGenericType(localEntityType)
                            .IsAssignableFrom(p.PropertyType));
        var dbSet = dbSetPropertyInfo.GetGetMethod()!.Invoke(context, null);
        
        var method = typeof(DbContextSaver)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.Name == nameof(GetEntityFromDatabaseWithPrimaryKey) && m.IsGenericMethod)
            .MakeGenericMethod(entityType);

        return method.Invoke(null, new object?[] { dbSet, change });
    }

    private static object? GetEntityFromDatabaseWithPrimaryKey<TEntity>(DbSet<TEntity> dbSet, Change change) where TEntity : class
    {
        IQueryable<TEntity> instance = dbSet;

        foreach (var primaryKeyField in change.PrimaryKeyFields!)
        {
            var parameterExpression = Expression.Parameter(typeof(TEntity));

            Expression left = Expression.Property(parameterExpression, primaryKeyField.Name);
            Expression right = Expression.Constant(primaryKeyField.Value);
            Expression equals = Expression.Equal(left, right);
            var lambda = (Expression<Func<TEntity, bool>>) Expression.Lambda(equals, parameterExpression);

            instance = instance.Where(lambda);
        }

        return instance.SingleOrDefault();
    }


    private static void ApplyFieldChanges(Change change, Type entityType, object? entity)
    {
        foreach (var fieldData in change.UpdatedFields!)
        {
            // For now, assume that we're only working with properties
            // TO DO - be able to handle properties that are backed by fields
            var setter = entityType.GetProperty(fieldData.Name)!.GetSetMethod()!;
            setter.Invoke(entity, new object?[] { fieldData.Value });
        }
    }
}
