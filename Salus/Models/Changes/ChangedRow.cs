using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace Salus.Models.Changes;

internal class ChangedRow
{
    public enum ChangeTypeEnum
    {
        Insert,
        Update,
        Delete
    }

    private EntityEntry? _entry;
    private PropertyValues? _originalValues;

    [JsonProperty]
    public ChangeTypeEnum ChangeType { get; private set; } = default;
    [JsonProperty]
    public string ChangeClrType { get; private set; } = string.Empty;
    [JsonProperty]
    public List<FieldWithValue>? UpdatedFields { get; private set; }
    [JsonProperty]
    public List<FieldWithValue>? PrimaryKeyFields { get; private set; }

    public ChangedRow()
    { }

    public ChangedRow(EntityEntry entry)
    {
        _entry = entry;
        _originalValues = entry.OriginalValues.Clone();
        ChangeType = entry.State switch
        {
            EntityState.Added => ChangeTypeEnum.Insert,
            EntityState.Modified => ChangeTypeEnum.Update,
            EntityState.Deleted => ChangeTypeEnum.Delete,
            _ => throw new InvalidOperationException("Entry's State is not suitable for building a Change record")
        };
        ChangeClrType = entry.Metadata.ClrType.AssemblyQualifiedName ?? throw new NullReferenceException("Attempt to track an entity with no Clr Type Name");
    }

    public void CompleteAfterSave()
    {
        switch (ChangeType)
        {
            case ChangeTypeEnum.Insert:
                UpdatedFields = GetAllFields();
                PrimaryKeyFields = GetPrimaryKeyFields();
                break;
            case ChangeTypeEnum.Update:
                UpdatedFields = GetChangedFields();
                PrimaryKeyFields = GetPrimaryKeyFields();
                break;
            case ChangeTypeEnum.Delete:
                PrimaryKeyFields = GetPrimaryKeyFields();
                break;
            default:
                throw new InvalidOperationException("Entry's ChangeType is not suitable for building a Change record");
        }
    }

    private List<FieldWithValue> GetPrimaryKeyFields()
    {
        if (_entry == null || _originalValues == null)
        {
            throw new InvalidOperationException("Can't finish getting data on an object that wasn't created from an Entity");
        }

        // Find primary key
        // Note that it is only possible to do this using internal EF Core features, which
        // may be removed in future releases of EF Core
#pragma warning disable EF1001 // Internal EF Core API usage.
        IDbContextServices contextServices = _entry.Context.GetService<IDbContextServices>();
        var internalEntityType = contextServices.Model.FindEntityType(_entry.Entity.GetType());
#pragma warning restore EF1001 // Internal EF Core API usage.
        var primaryKeyProperties = internalEntityType!.FindPrimaryKey()!.Properties;

        var results = new List<FieldWithValue>();

        foreach (var primaryKeyProperty in primaryKeyProperties)
        {
            results.Add(new FieldWithValue
            {
                Name = primaryKeyProperty.Name,
                Value = _entry.CurrentValues[primaryKeyProperty]
            });
        }

        return results;
    }

    private List<FieldWithValue> GetAllFields()
    {
        if (_entry == null || _originalValues == null)
        {
            throw new InvalidOperationException("Can't finish getting data on an object that wasn't created from an Entity");
        }

        var results = new List<FieldWithValue>();

        foreach (var property in _entry.Properties)
        {
            results.Add(new FieldWithValue
            {
                Name = property.Metadata.Name,
                Value = property.CurrentValue
            });
        }

        return results;
    }

    private List<FieldWithValue> GetChangedFields()
    {
        if (_entry == null || _originalValues == null)
        {
            throw new InvalidOperationException("Can't finish getting data on an object that wasn't created from an Entity");
        }

        var results = new List<FieldWithValue>();

        foreach (var property in _entry.Properties)
        {
            if (property.CurrentValue != _originalValues[property.Metadata])
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
