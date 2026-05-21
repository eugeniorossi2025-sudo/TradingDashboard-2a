using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents user's grid column display preferences.
/// </summary>
[Table("User_Grid_Configurations")]
public class UserGridConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    [Column("ID_user")]
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the page name (e.g., "Admin_Default_aspx").
    /// </summary>
    [Column("page_name")]
    [MaxLength(255)]
    public string? PageName { get; set; }

    /// <summary>
    /// Gets or sets the grid name.
    /// </summary>
    [Column("grid_name")]
    [MaxLength(255)]
    public string? GridName { get; set; }

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    [Column("column_name")]
    [MaxLength(255)]
    public string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets whether the column is visible (1=Visible, 0=Hidden).
    /// </summary>
    [Column("display")]
    public bool Display { get; set; }

    /// <summary>
    /// Navigation property to User.
    /// </summary>
    [ForeignKey("IdUser")]
    public User? User { get; set; }
}