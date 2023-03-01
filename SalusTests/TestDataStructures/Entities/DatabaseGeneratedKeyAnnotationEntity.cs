using System.ComponentModel.DataAnnotations.Schema;

namespace SalusTests.TestDataStructures.Entities;

internal class DatabaseGeneratedKeyAnnotationEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
}
