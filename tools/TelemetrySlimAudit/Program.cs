using System.Text.Json;
using Decisore.Engine;

static Telemetry BuildFullTelemetry(int botCount)
{
    var bots = new Dictionary<string, SecurityFilterBotTelemetry>();
    var margins = new Dictionary<string, double>();

    for (var i = 1; i <= botCount; i++)
    {
        var name = $"PC{i}";
        margins[name] = 84.15 + i;
        bots[name] = new SecurityFilterBotTelemetry
        {
            Computer = name,
            AvgHandSeconds = 22.5 + i,
            LastHandDeltaSeconds = 21.3,
            LastTwoHandDeltaSeconds = new[] { 22.1, 21.3 },
            MinHandDeltaSeconds = 18.0,
            MaxHandDeltaSeconds = 384.7,
            RapidL5TriggerActive = i % 2 == 0,
            L6PlayedCount = 2,
            LastL6DeltaSeconds = 4444.43,
            AvgL6DeltaSeconds = 4444.43,
            MinL6DeltaSeconds = 4444.43,
            MaxL6DeltaSeconds = 4444.43,
            LastL6DeltaHands = 114,
            AvgL6DeltaHands = 114,
            MinL6DeltaHands = 114,
            MaxL6DeltaHands = 114,
            L6DeltaSamples = 1,
            LastL6PlayedAtUtc = DateTime.UtcNow,
            LastL6PlayedPBHands = 143,
            AuthorizedL8LostCount = 0,
            LastAuthorizedL8LostDeltaSeconds = 0,
            LastL6AuthorizationAtUtc = DateTime.UtcNow,
            PBHandsPlayed = 166,
            LastL6AuthorizationPBHandsPlayed = 142,
            LastL6AuthorizationScore = 1,
            LastL6AuthorizationStreak = 5,
            LastL6AuthorizationShoeHand = 14,
            LastL6AuthorizationAvgHandSeconds = 22.0,
            CurrentStreak = 5,
            SecurityRiskScore = 2,
            SecurityFilterActive = false,
            PauseBot = false,
            PauseScope = "NONE",
            PreventedL6 = 0,
            LastShoeHand = 15,
            Martingala = 1,
            HasL6Credit = true,
            LastReason = "score 1/4",
            LastUpdatedUtc = DateTime.UtcNow,
            HandSamples = 8
        };
    }

    return new Telemetry
    {
        TotalPBHandsPlayed = 221,
        TotalAuthL6Authorized = 2,
        TotalL5Played = 8,
        TotalL5Won = 5,
        TotalL5Lost = 3,
        TotalL8Played = 0,
        TotalL8Won = 0,
        TotalL8Lost = 0,
        BotMargins = margins,
        SpotID = 1,
        SpotPBHandsPlayed = 21,
        SpotAuthL6Counter = 0,
        SpotL5Loss = 0,
        GlobalPauseScalping = false,
        GlobalPauseScalpingDetails = "Pausa non attiva",
        GlobalPauseScalpingDuration = "0",
        INC = 0,
        EWMA = 0,
        TotalPauseScalpingSoglieActivated = 0,
        TotalPauseScalpingEWMAActivated = 0,
        SecurityFilterEnabled = true,
        SecurityFilterMinScore = 3,
        TotalSecurityFilterActivated = 0,
        TotalSecurityFilterPreventedL6 = 0,
        LastAvgHandSeconds = 22.0,
        ActiveSecurityFilterBots = 0,
        SecurityFilterByBot = bots
    };
}

var failed = false;
Console.WriteLine("=== TELEMETRY SLIM AUDIT (TelemetryPersistence) ===");

foreach (var botCount in new[] { 1, 2, 4, 8 })
{
    var full = BuildFullTelemetry(botCount);
    var fullJson = JsonSerializer.Serialize(full);
    var persistence = TelemetryPersistence.From(full);
    var telemetryJson = JsonSerializer.Serialize(persistence);
    var securityFilterJson = JsonSerializer.Serialize(persistence.SecurityFilterByBot);
    var numeroBot = persistence.SecurityFilterByBot?.Count ?? 0;

    Console.WriteLine(
        $"TELEMETRY_SIZE telemetryJson.Length={telemetryJson.Length} numeroBot={numeroBot} dimensioneSecurityFilterByBot={securityFilterJson.Length}");

    var valid = true;
    try
    {
        JsonDocument.Parse(telemetryJson);
    }
    catch (Exception ex)
    {
        valid = false;
        Console.WriteLine($"  JSON INVALID: {ex.Message}");
        failed = true;
    }

    var truncated = telemetryJson.Length >= 4000;
    var fullWouldTruncate = fullJson.Length >= 4000;
    var status = valid && !truncated ? "PASS" : "FAIL";
    Console.WriteLine(
        $"  bots={botCount} slim={telemetryJson.Length} full={fullJson.Length} valid={valid} slim<=4000={!truncated} full_would_truncate={fullWouldTruncate} [{status}]");

    if (!valid || truncated)
        failed = true;

    // Global statistics fields present
    using var doc = JsonDocument.Parse(telemetryJson);
    var root = doc.RootElement;
    foreach (var field in new[] { "TotalPBHandsPlayed", "TotalAuthL6Authorized", "SecurityFilterByBot" })
    {
        if (!root.TryGetProperty(field, out _))
        {
            Console.WriteLine($"  MISSING FIELD: {field}");
            failed = true;
        }
    }

    if (root.TryGetProperty("SecurityFilterByBot", out var sf) && sf.GetPropertyCount() != botCount)
    {
        Console.WriteLine($"  SecurityFilterByBot count mismatch: expected {botCount}, got {sf.GetPropertyCount()}");
        failed = true;
    }
}

Console.WriteLine(failed ? "RESULT: FAIL" : "RESULT: PASS");
return failed ? 1 : 0;
