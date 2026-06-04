using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Controllers;

public static class SecurityFilterConfig
{
    public const string EnabledKey = "SECURITY_FILTER_ENABLED";
    public const string MinScoreKey = "SECURITY_FILTER_MIN_SCORE";
    public const string MaxAvgSecondsKey = "SECURITY_FILTER_MAX_AVG_SECONDS";
    public const string VeryFastSecondsKey = "SECURITY_FILTER_VERY_FAST_SECONDS";

    public const int DefaultMinScore = 3;
    public const decimal DefaultMaxAvgSeconds = 23.5m;
    public const decimal DefaultVeryFastSeconds = 21.0m;

    public static async Task<bool> GetEnabledAsync(AppDbContext context)
    {
        var value = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == EnabledKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(value))
            return true;

        return PlayerPaceFilterController.ParseEnabledFlag(value);
    }

    public static async Task SaveEnabledAsync(AppDbContext context, bool enabled)
    {
        await SaveConfigAsync(context, EnabledKey,
            enabled ? "1" : "0",
            "Security Filter per-bot: 1 attivo, 0 spento.",
            900);
    }

    public static async Task<SecurityFilterParametersDto> GetParametersAsync(AppDbContext context)
    {
        var keys = new[] { MinScoreKey, MaxAvgSecondsKey, VeryFastSecondsKey };
        var configs = await context.Configurations.AsNoTracking()
            .Where(c => keys.Contains(c.Key))
            .ToDictionaryAsync(c => c.Key, c => c.Value);

        var minScore = DefaultMinScore;
        if (configs.TryGetValue(MinScoreKey, out var ms) &&
            int.TryParse(ms, out var parsedMs) && parsedMs >= 1 && parsedMs <= 4)
            minScore = parsedMs;

        var maxAvg = DefaultMaxAvgSeconds;
        if (configs.TryGetValue(MaxAvgSecondsKey, out var ma) &&
            decimal.TryParse(ma, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsedMa) &&
            parsedMa > 0)
            maxAvg = parsedMa;

        var veryFast = DefaultVeryFastSeconds;
        if (configs.TryGetValue(VeryFastSecondsKey, out var vf) &&
            decimal.TryParse(vf, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsedVf) &&
            parsedVf > 0)
            veryFast = parsedVf;

        return new SecurityFilterParametersDto
        {
            MaxAvgSeconds = maxAvg,
            VeryFastSeconds = veryFast,
            MinScore = minScore
        };
    }

    public static async Task<SecurityFilterParametersDto> SaveParametersAsync(
        AppDbContext context,
        decimal maxAvgSeconds,
        decimal veryFastSeconds,
        int minScore)
    {
        await SaveConfigAsync(context, MaxAvgSecondsKey, maxAvgSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Security Filter: media secondi mano sotto questa soglia aumenta il rischio.", 903);
        await SaveConfigAsync(context, VeryFastSecondsKey, veryFastSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Security Filter: media secondi mano molto veloce, ulteriore punto rischio.", 904);
        await SaveConfigAsync(context, MinScoreKey, minScore.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Security Filter: score minimo su 4 per mettere in pausa solo quel bot.", 906);

        return new SecurityFilterParametersDto
        {
            MaxAvgSeconds = maxAvgSeconds,
            VeryFastSeconds = veryFastSeconds,
            MinScore = minScore
        };
    }

    static async Task SaveConfigAsync(AppDbContext context, string key, string value, string description, int pos)
    {
        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        if (setting == null)
        {
            setting = new Configuration { Key = key, Description = description, Pos = pos, Value = value };
            context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = value;
        }

        await context.SaveChangesAsync();
    }
}

public class SecurityFilterParametersDto
{
    public decimal MaxAvgSeconds { get; set; }
    public decimal VeryFastSeconds { get; set; }
    public int MinScore { get; set; }
}

public class SecurityFilterConfigResponse
{
    public bool Enabled { get; set; }
    public decimal MaxAvgSeconds { get; set; }
    public decimal VeryFastSeconds { get; set; }
    public int MinScore { get; set; }
}

public class SecurityFilterEnabledRequest
{
    public bool? Enabled { get; set; }
}

public class SecurityFilterEnabledResponse
{
    public bool Enabled { get; set; }
}

public class SecurityFilterParametersRequest
{
    public decimal? MaxAvgSeconds { get; set; }
    public decimal? VeryFastSeconds { get; set; }
    public int? MinScore { get; set; }
}
