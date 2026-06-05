namespace Decisore.Engine;

/// <summary>
/// Slim telemetry persisted to dbo.Statistiche.TELEMETRY (NVARCHAR(4000)).
/// Full per-bot detail is served on-demand via GET security-filter/{computer}.
/// </summary>
public class TelemetryPersistence
{
    public int TotalPBHandsPlayed { get; set; }
    public int TotalAuthL6Authorized { get; set; }
    public int TotalL5Played { get; set; }
    public int TotalL5Won { get; set; }
    public int TotalL5Lost { get; set; }
    public int TotalL8Played { get; set; }
    public int TotalL8Won { get; set; }
    public int TotalL8Lost { get; set; }
    public Dictionary<string, double> BotMargins { get; set; } = new();

    public int SpotID { get; set; }
    public int SpotPBHandsPlayed { get; set; }
    public int SpotAuthL6Counter { get; set; }
    public int SpotL5Loss { get; set; }

    public bool GlobalPauseScalping { get; set; }
    public string GlobalPauseScalpingDetails { get; set; } = "Pausa non attiva";
    public string GlobalPauseScalpingDuration { get; set; } = "0";

    public double INC { get; set; }
    public double EWMA { get; set; }

    public int TotalPauseScalpingSoglieActivated { get; set; }
    public int TotalPauseScalpingEWMAActivated { get; set; }

    public int TotalSecurityFilterActivated { get; set; }
    public int TotalSecurityFilterPreventedL6 { get; set; }
    public double LastAvgHandSeconds { get; set; }
    public int ActiveSecurityFilterBots { get; set; }

    public Dictionary<string, SecurityFilterBotSummary> SecurityFilterByBot { get; set; } = new();

    public static TelemetryPersistence From(Telemetry source)
    {
        var botKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (source.BotMargins != null)
        {
            foreach (var key in source.BotMargins.Keys)
                botKeys.Add(key);
        }

        if (source.SecurityFilterByBot != null)
        {
            foreach (var key in source.SecurityFilterByBot.Keys)
                botKeys.Add(key);
        }

        var summaries = new Dictionary<string, SecurityFilterBotSummary>(StringComparer.OrdinalIgnoreCase);
        foreach (var computer in botKeys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            if (source.SecurityFilterByBot != null &&
                source.SecurityFilterByBot.TryGetValue(computer, out var full))
            {
                summaries[computer] = SecurityFilterBotSummary.From(full);
            }
        }

        return new TelemetryPersistence
        {
            TotalPBHandsPlayed = source.TotalPBHandsPlayed,
            TotalAuthL6Authorized = source.TotalAuthL6Authorized,
            TotalL5Played = source.TotalL5Played,
            TotalL5Won = source.TotalL5Won,
            TotalL5Lost = source.TotalL5Lost,
            TotalL8Played = source.TotalL8Played,
            TotalL8Won = source.TotalL8Won,
            TotalL8Lost = source.TotalL8Lost,
            BotMargins = source.BotMargins != null
                ? new Dictionary<string, double>(source.BotMargins)
                : new Dictionary<string, double>(),
            SpotID = source.SpotID,
            SpotPBHandsPlayed = source.SpotPBHandsPlayed,
            SpotAuthL6Counter = source.SpotAuthL6Counter,
            SpotL5Loss = source.SpotL5Loss,
            GlobalPauseScalping = source.GlobalPauseScalping,
            GlobalPauseScalpingDetails = source.GlobalPauseScalpingDetails,
            GlobalPauseScalpingDuration = source.GlobalPauseScalpingDuration,
            INC = source.INC,
            EWMA = source.EWMA,
            TotalPauseScalpingSoglieActivated = source.TotalPauseScalpingSoglieActivated,
            TotalPauseScalpingEWMAActivated = source.TotalPauseScalpingEWMAActivated,
            TotalSecurityFilterActivated = source.TotalSecurityFilterActivated,
            TotalSecurityFilterPreventedL6 = source.TotalSecurityFilterPreventedL6,
            LastAvgHandSeconds = source.LastAvgHandSeconds,
            ActiveSecurityFilterBots = source.ActiveSecurityFilterBots,
            SecurityFilterByBot = summaries
        };
    }
}

public class SecurityFilterBotSummary
{
    public double AvgHandSeconds { get; set; }
    public double LastHandDeltaSeconds { get; set; }
    public double[] LastTwoHandDeltaSeconds { get; set; } = Array.Empty<double>();
    public bool RapidL5TriggerActive { get; set; }
    public int CurrentStreak { get; set; }
    public int SecurityRiskScore { get; set; }
    public bool SecurityFilterActive { get; set; }
    public bool PlayerRace5Alert { get; set; }
    public bool PlayerRace5Triggered { get; set; }
    public bool PlayerRace5Ac3Triggered { get; set; }
    public bool PlayerRace8Alert { get; set; }
    public bool PlayerRace8Ac3Triggered { get; set; }
    public bool PlayerPaceRiskActive { get; set; }
    public bool PlayerPaceTriggeredAC3 { get; set; }
    public bool PauseBot { get; set; }
    public string PauseScope { get; set; } = "NONE";
    public string PauseComputer { get; set; } = "";
    public int PreventedL6 { get; set; }
    public int LastShoeHand { get; set; }
    public int Martingala { get; set; }
    public bool HasL6Credit { get; set; }
    public string LastReason { get; set; } = "";
    public int L6PlayedCount { get; set; }
    public int AuthorizedL8LostCount { get; set; }
    public string CurrentStreakOutcome { get; set; } = "";
    public int PlayerStreakCount { get; set; }
    public double PlayerStreakP1ToP5TotalSeconds { get; set; }
    public double PlayerStreakMeanIntervalSeconds { get; set; }
    public double[] PlayerStreakIntervalSeconds { get; set; } = Array.Empty<double>();
    public int SpotPbHandsPlayed { get; set; }
    public int SpotL5PlayedCount { get; set; }
    public int SpotL5LossCount { get; set; }
    public int SpotL6CreditBalance { get; set; }
    public int SpotL6GrantedCount { get; set; }
    public bool SpotL6Authorized { get; set; }
    public bool NextL5LossWillAuthorizeL6 { get; set; }
    public int SpotCycleId { get; set; } = 1;

    public static SecurityFilterBotSummary From(SecurityFilterBotTelemetry source) =>
        new()
        {
            AvgHandSeconds = source.AvgHandSeconds,
            LastHandDeltaSeconds = source.LastHandDeltaSeconds,
            LastTwoHandDeltaSeconds = source.LastTwoHandDeltaSeconds?.ToArray() ?? Array.Empty<double>(),
            RapidL5TriggerActive = source.RapidL5TriggerActive,
            CurrentStreak = source.CurrentStreak,
            CurrentStreakOutcome = source.CurrentStreakOutcome ?? "",
            PlayerStreakCount = source.PlayerStreakCount,
            PlayerStreakP1ToP5TotalSeconds = source.PlayerStreakP1ToP5TotalSeconds,
            PlayerStreakMeanIntervalSeconds = source.PlayerStreakMeanIntervalSeconds,
            PlayerStreakIntervalSeconds = source.PlayerStreakIntervalSeconds?.ToArray() ?? Array.Empty<double>(),
            SecurityRiskScore = source.SecurityRiskScore,
            SecurityFilterActive = source.SecurityFilterActive,
            PlayerRace5Alert = source.PlayerRace5Alert,
            PlayerRace5Triggered = source.PlayerRace5Alert,
            PlayerRace5Ac3Triggered = source.PlayerRace5Ac3Triggered,
            PlayerRace8Alert = source.PlayerRace8Alert,
            PlayerRace8Ac3Triggered = source.PlayerRace8Ac3Triggered,
            PlayerPaceRiskActive = source.PlayerPaceRiskActive,
            PlayerPaceTriggeredAC3 = source.PlayerPaceTriggeredAC3,
            PauseBot = source.PauseBot,
            PauseScope = source.PauseScope,
            PauseComputer = source.PauseComputer,
            PreventedL6 = source.PreventedL6,
            LastShoeHand = source.LastShoeHand,
            Martingala = source.Martingala,
            HasL6Credit = source.HasL6Credit,
            LastReason = source.LastReason,
            L6PlayedCount = source.L6PlayedCount,
            AuthorizedL8LostCount = source.AuthorizedL8LostCount,
            SpotPbHandsPlayed = source.SpotPbHandsPlayed,
            SpotL5PlayedCount = source.SpotL5PlayedCount,
            SpotL5LossCount = source.SpotL5LossCount,
            SpotL6CreditBalance = source.SpotL6CreditBalance,
            SpotL6GrantedCount = source.SpotL6GrantedCount,
            SpotL6Authorized = source.SpotL6Authorized,
            NextL5LossWillAuthorizeL6 = source.NextL5LossWillAuthorizeL6,
            SpotCycleId = source.SpotCycleId
        };
}
