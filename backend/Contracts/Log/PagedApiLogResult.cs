namespace Contracts.Log;

public class PagedApiLogResult
{
    public IList<ApiLogDto> Items { get; set; } = new List<ApiLogDto>();

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}

public class ApiLogDto
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public int Action { get; set; }

    public DateTime CreatedAt { get; set; }
}
