using Entities;

using Microsoft.EntityFrameworkCore;

using WebApi.Data;



namespace WebApi.Controllers;



public static class SpotResetConfig

{

    public const string ThresholdKey = "SPOT_RESET_THRESHOLD_L5";

    public const string CyclePbHandsKey = "SPOT_CYCLE_PB_HANDS";

    public const string PerBotEnabledKey = "SPOT_L6_PER_BOT_ENABLED";

    public const string CreditL5RequiredKey = "SPOT_L6_CREDIT_L5_REQUIRED";

    public const string CreditsGeneratedKey = "SPOT_L6_CREDITS_GENERATED";

    public const int DefaultThreshold = 2;

    public const int DefaultCyclePbHands = 600;

    public const int DefaultCreditsGenerated = 1;



    public static async Task<int> GetThresholdAsync(AppDbContext context)

    {

        var value = await context.Configurations.AsNoTracking()

            .Where(c => c.Key == ThresholdKey)

            .Select(c => c.Value)

            .FirstOrDefaultAsync();



        if (int.TryParse(value, out var parsed) && parsed >= 1)

            return parsed;



        return DefaultThreshold;

    }



    public static async Task SaveThresholdAsync(AppDbContext context, int threshold)

    {

        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == ThresholdKey);

        if (setting == null)

        {

            setting = new Configuration

            {

                Key = ThresholdKey,

                Description = "Soglia L6 per bot: dopo N L5 persi nel ciclo SPOT, solo quel bot può passare a L6.",

                Pos = 914,

                Value = threshold.ToString()

            };

            context.Configurations.Add(setting);

        }

        else

        {

            setting.Value = threshold.ToString();

        }



        await context.SaveChangesAsync();

    }

    public static async Task<int> GetCyclePbHandsAsync(AppDbContext context)
    {
        var primary = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == CyclePbHandsKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        if (int.TryParse(primary, out var parsed) && parsed >= 1)
            return parsed;

        var legacy = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == "L6_AUTH_PB_RESET_COUNTER")
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        if (int.TryParse(legacy, out var legacyParsed) && legacyParsed >= 1)
            return legacyParsed;

        return DefaultCyclePbHands;
    }

    public static async Task SaveCyclePbHandsAsync(AppDbContext context, int hands)
    {
        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == CyclePbHandsKey);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = CyclePbHandsKey,
                Description = "Mani PB globali per ciclo SPOT; alla soglia si chiude il ciclo e si azzerano i contatori per-bot.",
                Pos = 915,
                Value = hands.ToString()
            };
            context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = hands.ToString();
        }

        await context.SaveChangesAsync();
    }

    public static bool ParsePerBotEnabledFlag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<bool> GetPerBotEnabledAsync(AppDbContext context)
    {
        var value = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == PerBotEnabledKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return ParsePerBotEnabledFlag(value);
    }

    public static async Task SavePerBotEnabledAsync(AppDbContext context, bool enabled)
    {
        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == PerBotEnabledKey);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = PerBotEnabledKey,
                Description = "SPOT L6 per bot: 1 attivo (L6 dopo N L5 perse nel ciclo SPOT del bot), 0 spento.",
                Pos = 916,
                Value = enabled ? "1" : "0"
            };
            context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = enabled ? "1" : "0";
        }

        await context.SaveChangesAsync();
    }

    public static async Task<int> GetCreditL5RequiredAsync(AppDbContext context)
    {
        var value = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == CreditL5RequiredKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        if (int.TryParse(value, out var parsed) && parsed >= 1)
            return parsed;

        return await GetThresholdAsync(context);
    }

    public static async Task<int> GetCreditsGeneratedAsync(AppDbContext context)
    {
        var value = await context.Configurations.AsNoTracking()
            .Where(c => c.Key == CreditsGeneratedKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        if (int.TryParse(value, out var parsed) && parsed >= 1)
            return parsed;

        return DefaultCreditsGenerated;
    }

    public static async Task<(int CreditL5Required, int CreditsGenerated)> GetCreditsAsync(AppDbContext context)
    {
        var required = await GetCreditL5RequiredAsync(context);
        var generated = await GetCreditsGeneratedAsync(context);
        return (required, generated);
    }

    public static async Task SaveCreditsAsync(AppDbContext context, int creditL5Required, int creditsGenerated)
    {
        await SaveConfigIntAsync(
            context,
            CreditL5RequiredKey,
            creditL5Required,
            "SPOT L6 crediti per bot: L5 perse richieste per generare crediti.",
            917);

        await SaveConfigIntAsync(
            context,
            CreditsGeneratedKey,
            creditsGenerated,
            "SPOT L6 crediti per bot: crediti generati per ogni milestone L5.",
            918);
    }

    static async Task SaveConfigIntAsync(
        AppDbContext context,
        string key,
        int value,
        string description,
        int pos)
    {
        var setting = await context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = key,
                Description = description,
                Pos = pos,
                Value = value.ToString()
            };
            context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = value.ToString();
        }

        await context.SaveChangesAsync();
    }

}



public class SpotResetThresholdRequest

{

    public int? Threshold { get; set; }

}



public class SpotResetThresholdResponse

{

    public int Threshold { get; set; }

}


