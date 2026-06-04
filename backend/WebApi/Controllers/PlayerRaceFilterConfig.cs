using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Controllers;

public static class PlayerRaceFilterConfig
{
    public const string Race5FilterKey = "PLAYER_RACE_5_FILTER_ENABLED";
    public const string Race5Ac3Key = "PLAYER_RACE_5_AC3_ENABLED";
    public const string Race8FilterKey = "PLAYER_RACE_8_FILTER_ENABLED";
    public const string Race8Ac3Key = "PLAYER_RACE_8_AC3_ENABLED";

    public static async Task<bool> GetAsync(AppDbContext context, string key)
    {
        var value = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == key).Select(c => c.Value).FirstOrDefaultAsync();
        return PlayerPaceFilterController.ParseEnabledFlag(value);
    }

    public static async Task SaveAsync(AppDbContext context, string key, string description, int pos, bool enabled)
    {
        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        if (setting == null)
        {
            setting = new Configuration { Key = key, Description = description, Pos = pos, Value = enabled ? "1" : "0" };
            context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = enabled ? "1" : "0";
        }

        await context.SaveChangesAsync();
    }
}

public class PlayerRaceFilterRequest
{
    public bool? Enabled { get; set; }
}

public class PlayerRaceFilterResponse
{
    public bool Enabled { get; set; }
}
