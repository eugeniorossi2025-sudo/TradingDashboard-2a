using Contracts.Log;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between Log entity and DTOs.
/// </summary>
public static class LogMappingExtensions
{
    /// <summary>
    /// Maps a CreateLogRequest to a Log entity.
    /// </summary>
    /// <param name="request">The create log request.</param>
    /// <returns>A new Log entity.</returns>
    public static Entities.Log ToEntity(this CreateLogRequest request)
    {
        return new Entities.Log
        {
            DateTime = request.DateTime,
            Margine = request.Margine,
            Notes = request.Notes,
            Json = request.Json
        };
    }

    /// <summary>
    /// Updates a Log entity from an UpdateLogRequest.
    /// </summary>
    /// <param name="log">The log entity to update.</param>
    /// <param name="request">The update log request.</param>
    public static void UpdateFromRequest(this Entities.Log log, UpdateLogRequest request)
    {
        log.DateTime = request.DateTime;
        log.Margine = request.Margine;
        log.Notes = request.Notes;
        log.Json = request.Json;
    }
}