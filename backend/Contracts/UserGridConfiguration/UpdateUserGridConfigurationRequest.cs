// Contracts/UserGridConfiguration/UpdateUserGridConfigurationRequest.cs

namespace Contracts.UserGridConfiguration;

/// <summary>
/// Represents a request to update a user grid configuration.
/// </summary>
public class UpdateUserGridConfigurationRequest
{
    /// <summary>
    /// Gets or sets the page name.
    /// </summary>
    public string? PageName { get; set; }

    /// <summary>
    /// Gets or sets the grid name.
    /// </summary>
    public string? GridName { get; set; }

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets whether the column is visible.
    /// </summary>
    public bool Display { get; set; }
}