using Contracts.Value;
using Entities;

namespace WebApi.Extensions.Mapping;

public static class ValueMappingExtensions
{
    public static Values MapToEntity(this CreateValueRequest request)
    {
        return new Values
        {
            Key = decimal.TryParse(request.Key, out var parsedKey) ? parsedKey : null,
            Description = request.Description,
            Value = request.Value,
            IdUser = request.IdUser,
            DateTime = DateTime.Now
        };
    }

    public static void UpdateFromRequest(this Values value, UpdateValueRequest request)
    {
        if (request.Description != null) value.Description = request.Description;
        if (request.Value != null) value.Value = request.Value;
    }

    public static ValueResponse ToContract(this Values value)
    {
        return new ValueResponse
        {
            Id = (int)value.Id,
            Key = value.Key?.ToString() ?? string.Empty,
            Description = value.Description,
            Value = value.Value,
            IdUser = value.IdUser ?? 0,
            DateTime = value.DateTime ?? DateTime.MinValue
        };
    }
}
