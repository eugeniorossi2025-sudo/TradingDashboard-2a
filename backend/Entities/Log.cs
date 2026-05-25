using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Production API log entry (dbo.ApiLogs).
/// </summary>
[Table("ApiLogs")]
public class Log
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("Category")]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Column("Action")]
    public int Action { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
