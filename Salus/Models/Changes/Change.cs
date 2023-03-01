using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace Salus.Models.Changes;

public class Change
{
    public enum ChangeTypeEnum
    {
        Insert,
        Update,
        Delete
    }

    [JsonProperty]
    public ChangeTypeEnum ChangeType { get; private set; } = default;
    [JsonProperty]
    public string ChangeClrType { get; private set; } = string.Empty;
    [JsonProperty]
    public List<FieldWithValue>? UpdatedFields { get; private set; }
    [JsonProperty]
    public List<FieldWithValue>? PrimaryKeyFields { get; private set; }

    public Change()
    { }

    public Change(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                ChangeType = ChangeTypeEnum.Insert;
                ChangeClrType = entry.Metadata.ClrType.AssemblyQualifiedName ?? throw new NullReferenceException("Attempt to track an entity with no Clr Type Name");
                UpdatedFields = GetAllFields(entry);
                PrimaryKeyFields = GetPrimaryKeyFields(entry);
                break;
            case EntityState.Modified:
                ChangeType = ChangeTypeEnum.Update;
                ChangeClrType = entry.Metadata.ClrType.AssemblyQualifiedName ?? throw new NullReferenceException("Attempt to track an entity with no Clr Type Name");
                UpdatedFields = GetChangedFields(entry);
                PrimaryKeyFields = GetPrimaryKeyFields(entry);
                break;
            case EntityState.Deleted:
                ChangeType = ChangeTypeEnum.Delete;
                ChangeClrType = entry.Metadata.ClrType.AssemblyQualifiedName ?? throw new NullReferenceException("Attempt to track an entity with no Clr Type Name");
                PrimaryKeyFields = GetPrimaryKeyFields(entry);
                break;
            default:
                throw new InvalidOperationException("Entry's State is not suitable for building a Change record");
        }
    }

    private List<FieldWithValue> GetPrimaryKeyFields(EntityEntry entry)
    {
        // Find primary key
        // Note that it is only possible to do this using internal EF Core features, which
        // may be removed in future releases of EF Core
#pragma warning disable EF1001 // Internal EF Core API usage.
        IDbContextServices contextServices = entry.Context.GetService<IDbContextServices>();
        var internalEntityType = contextServices.Model.FindEntityType(entry.Entity.GetType());
#pragma warning restore EF1001 // Internal EF Core API usage.
        var primaryKeyProperties = internalEntityType!.FindPrimaryKey()!.Properties;

        var results = new List<FieldWithValue>();

        foreach (var primaryKeyProperty in primaryKeyProperties)
        {
            results.Add(new FieldWithValue
            {
                Name = primaryKeyProperty.Name,
                Value = entry.OriginalValues[primaryKeyProperty]
            });
        }

        return results;
    }

    private List<FieldWithValue> GetAllFields(EntityEntry entry)
    {
        var results = new List<FieldWithValue>();

        foreach (var property in entry.Properties)
        {
            results.Add(new FieldWithValue
            {
                Name = property.Metadata.Name,
                Value = property.CurrentValue
            });
        }

        return results;
    }

    private List<FieldWithValue> GetChangedFields(EntityEntry entry)
    {
        var results = new List<FieldWithValue>();

        foreach (var property in entry.Properties)
        {
            if (property.CurrentValue != property.OriginalValue)
            {
                results.Add(new FieldWithValue
                {
                    Name = property.Metadata.Name,
                    Value = property.CurrentValue
                });
            }
        }

        return results;
    }
}
