using Contracts.Log;

namespace WebApi.Extensions.Mapping;

public static class LogMappingExtensions
{
    public static Entities.Log ToEntity(this CreateLogRequest request)
    {
        return new Entities.Log
        {
            Description = request.Description,
            Category = request.Category,
            Action = request.Action,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
        };
    }

    public static void UpdateFromRequest(this Entities.Log log, UpdateLogRequest request)
    {
        log.Description = request.Description;
        log.Category = request.Category;
        log.Action = request.Action;
        log.CreatedAt = request.CreatedAt;
    }

    public static ApiLogDto ToDto(this Entities.Log log)
    {
        return new ApiLogDto
        {
            Id = log.Id,
            Description = log.Description,
            Category = log.Category,
            Action = log.Action,
            CreatedAt = log.CreatedAt,
        };
    }
}
