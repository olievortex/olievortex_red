using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class StormEventsDatabaseInventoryEntity
{
    [MaxLength(36)] public string Id { get; init; } = string.Empty;
    public int Year { get; init; }

    [MaxLength(320)] public string BlobName { get; init; } = string.Empty;
    public int RowCount { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
}