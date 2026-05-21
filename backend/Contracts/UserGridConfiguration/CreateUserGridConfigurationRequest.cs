// Contracts/UserGridConfiguration/CreateUserGridConfigurationRequest.cs

namespace Contracts.UserGridConfiguration;

/// <summary>
/// Represents a request to create a new user grid configuration.
/// </summary>
public class CreateUserGridConfigurationRequest
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the page name.
    /// </summary>
    public string PageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the grid name.
    /// </summary>
    public string GridName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column is visible.
    /// </summary>
    public bool Display { get; set; }
}