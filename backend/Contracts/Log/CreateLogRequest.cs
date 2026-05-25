namespace Contracts.Log;

public class CreateLogRequest
{
    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Action { get; set; }

    public DateTime? CreatedAt { get; set; }
}
