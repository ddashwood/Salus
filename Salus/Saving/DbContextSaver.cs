using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salus.Models.Changes;
using System.Linq.Expressions;
using System.Reflection;

namespace Salus.Saving;

internal class DbContextSaver<TKey> : IDbContextSaver<TKey>
{
    // When applying changes from a collection of Change objects, we need to ensure that we
    // don't add those changes into the SalusDataChanges table in the database - the changes have
    // come from elsewhere in the system, and the Updates table only references the initial
    // change of the data, not when it is updated elsewhere
    private bool _applying;

    private readonly ILogger<DbContextSaver<TKey>> _logger;

    public DbContextSaver(ILogger<DbContextSaver<TKey>> logger)
    {
        _logger = logger;
    }

    public Save<TKey>? BuildPreliminarySave(DbContext context)
    {
        if (_applying)
        {
            return null;
        }

        context.ChangeTracker.DetectChanges();

        List<ChangedRow> changes = new();

        var changeTrackerEntries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in changeTrackerEntries)
        {
            var dbSet = context.GetType()
                .GetProperties()
                .SingleOrDefault(p => p.PropertyType == typeof(DbSet<>).MakeGenericType(entry.Entity.GetType()));

            if (dbSet == null)
            {
                continue;
            }

            var attribute = dbSet.GetCustomAttribute<SalusSourceDbSetAttribute>();
            if (attribute == null)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    changes.Add (new ChangedRow(entry, attribute.SalusName));
                    break;
            }
        }

        if (changes.Count == 0)
        {
            return null;
        }

        return new Save<TKey>(changes);
    }

    public Task<Save<TKey>?> BuildPreliminarySaveAsync(CancellationToken cancellationToken, DbContext context)
    {
        return Task.FromResult(BuildPreliminarySave(context));
    }

    public void Apply(DbContext context, IEnumerable<ChangedRow> changes)
    {
        try
        {
            _applying = true;

            foreach (var change in changes)
            {
                switch (change.ChangeType)
                {
                    case ChangedRow.ChangeTypeEnum.Insert:
                        ApplyInsert(context, change);
                        break;
                    case ChangedRow.ChangeTypeEnum.Update:
                        ApplyUpdate(context, change);
                        break;
                    case ChangedRow.ChangeTypeEnum.Delete:
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

    private void ApplyInsert(DbContext context, ChangedRow change)
    {
        var existingEntity = GetEntityFromDatabaseWithPrimaryKey(context, change, out var entityType);
        if (existingEntity != null)
        {
            return; // This entity already exists in the database
        }
        if (entityType == null)
        {
            return;
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

    private void ApplyUpdate(DbContext context, ChangedRow change)
    {
        var entity = GetEntityFromDatabaseWithPrimaryKey(context, change, out var entityType);
        if (entityType == null)
        {
            return;
        }
        ApplyFieldChanges(change, entityType, entity);
    }

    private void ApplyDelete(DbContext context, ChangedRow change)
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
                            .MakeGenericType(entityType!)
                            .IsAssignableFrom(p.PropertyType));
        var dbSet = dbSetPropertyInfo.GetGetMethod()!.Invoke(context, null);
        dbSetPropertyInfo.PropertyType.GetMethod("Remove")!.Invoke(dbSet, new object?[] { entity });
    }

    private object? GetEntityFromDatabaseWithPrimaryKey(DbContext context, ChangedRow change, out Type? entityType)
    {
        // N.b. need to create a local copy of this, because we are not allowed to use "out" parameters in
        // lambdas such as when we get the dbSetPropertyInfo below

        entityType = null;

        var dbSetPropertyInfo = context
            .GetType()
            .GetProperties()
            .SingleOrDefault(p =>
            {
                if (!p.PropertyType.IsGenericType || p.PropertyType.GetGenericTypeDefinition() != typeof(DbSet<>))
                {
                    return false;
                }

                var attribute = p.GetCustomAttribute<SalusDestinationDbSetAttribute>();
                if (attribute == null)
                {
                    return false;
                }

                var salusType = attribute.SalusName ?? p.PropertyType.GenericTypeArguments[0].Name;

                return salusType == change.ChangeSalusType;
            });

        if (dbSetPropertyInfo == null)
        {
            _logger.LogWarning("No DbSet found in which to write data for type " + change.ChangeSalusType);
            return null;
        }

        entityType = dbSetPropertyInfo.PropertyType.GenericTypeArguments[0];
        var dbSet = dbSetPropertyInfo.GetGetMethod()!.Invoke(context, null);
        
        var method = typeof(DbContextSaver<TKey>)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.Name == nameof(GetEntityFromDatabaseWithPrimaryKey) && m.IsGenericMethod)
            .MakeGenericMethod(entityType);

        return method.Invoke(null, new object?[] { dbSet, change });
    }

    private static object? GetEntityFromDatabaseWithPrimaryKey<TEntity>(DbSet<TEntity> dbSet, ChangedRow change) where TEntity : class
    {
        IQueryable<TEntity> instance = dbSet;

        foreach (var primaryKeyField in change.PrimaryKeyFields!)
        {
            var destinationType = typeof(TEntity).GetProperty(primaryKeyField.Name)!.PropertyType;
            var value = Convert.ChangeType(primaryKeyField.Value, destinationType);

            var parameterExpression = Expression.Parameter(typeof(TEntity));
            
            Expression left = Expression.Property(parameterExpression, primaryKeyField.Name);
            Expression right = Expression.Constant(value);
            Expression equals = Expression.Equal(left, right);
            var lambda = (Expression<Func<TEntity, bool>>) Expression.Lambda(equals, parameterExpression);

            instance = instance.Where(lambda);
        }

        return instance.SingleOrDefault();
    }


    private static void ApplyFieldChanges(ChangedRow change, Type entityType, object? entity)
    {
        if (entity == null)
        {
            return;
        }

        foreach (var fieldData in change.UpdatedFields!)
        {
            var prop = entityType.GetProperty(fieldData.Name);
            if (prop == null)
            {
                return; // Property does not exist on the destination type
            }

            // For now, assume that we're only working with properties
            // TO DO - be able to handle properties that are backed by fields

            var destinationType = prop.PropertyType;
            var value = Convert.ChangeType(fieldData.Value, destinationType);

            var setter = prop.GetSetMethod()!;
            setter.Invoke(entity, new object?[] { value });
        }
    }
}
