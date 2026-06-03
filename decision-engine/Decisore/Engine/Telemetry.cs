namespace Decisore.Engine;

public class Telemetry
{
    public int TotalPBHandsPlayed { get; set; } = 0;
    public int TotalAuthL6Authorized { get; set; } = 0;
    public int TotalL5Played { get; set; } = 0;
    public int TotalL5Won { get; set; } = 0;
    public int TotalL5Lost { get; set; } = 0;
    public int TotalL8Played { get; set; } = 0;
    public int TotalL8Won { get; set; } = 0;
    public int TotalL8Lost { get; set; } = 0;
    public Dictionary<string, double> BotMargins { get; set; }
    
    public int SpotID { get; set; } = 0;
    public int SpotPBHandsPlayed { get; set; } = 0;
    public int SpotAuthL6Counter { get; set; } = 0;
    public int SpotL5Loss { get; set; } = 0;
    
    public bool GlobalPauseScalping { get; set; } = false;
    public string GlobalPauseScalpingDetails { get; set; } = "Pausa non attiva";
    public string GlobalPauseScalpingDuration { get; set; } = "0";
    
    public double INC { get; set; } = 0;
    public double EWMA { get; set; } = 0;
    
    public int TotalPauseScalpingSoglieActivated { get; set; } = 0;
    public int TotalPauseScalpingEWMAActivated { get; set; } = 0;

    // Security Filter — filtro sperimentale compressione temporale streak
    public bool   SecurityFilterEnabled          { get; set; } = true;
    public int    SecurityFilterMinScore         { get; set; } = 3;
    public int    SecurityFilterMinStreak        { get; set; } = 5;
    public int    SecurityFilterMaxShoeHand      { get; set; } = 20;
    public double SecurityFilterMaxAvgSeconds    { get; set; } = 25.85;
    public double SecurityFilterVeryFastSeconds  { get; set; } = 23.1;
    public int    SecurityFilterDeltaWindow      { get; set; } = 8;
    public int    TotalSecurityFilterActivated    { get; set; } = 0;
    public int    TotalSecurityFilterPreventedL6  { get; set; } = 0;
    public double LastAvgHandSeconds              { get; set; } = 0;
    public int    ActiveSecurityFilterBots        { get; set; } = 0;
    public Dictionary<string, SecurityFilterBotTelemetry> SecurityFilterByBot { get; set; } = new();
}

public class SecurityFilterBotTelemetry
{
    public string Computer { get; set; } = "";
    public double AvgHandSeconds { get; set; }
    public double LastHandDeltaSeconds { get; set; }
    public double[] LastTwoHandDeltaSeconds { get; set; } = Array.Empty<double>();
    public double MinHandDeltaSeconds { get; set; }
    public double MaxHandDeltaSeconds { get; set; }
    public bool RapidL5TriggerActive { get; set; }
    public int L6PlayedCount { get; set; }
    public double LastL6DeltaSeconds { get; set; }
    public double AvgL6DeltaSeconds { get; set; }
    public double MinL6DeltaSeconds { get; set; }
    public double MaxL6DeltaSeconds { get; set; }
    public int LastL6DeltaHands { get; set; }
    public double AvgL6DeltaHands { get; set; }
    public int MinL6DeltaHands { get; set; }
    public int MaxL6DeltaHands { get; set; }
    public int L6DeltaSamples { get; set; }
    public DateTime LastL6PlayedAtUtc { get; set; }
    public int LastL6PlayedPBHands { get; set; }
    public int AuthorizedL8LostCount { get; set; }
    public double LastAuthorizedL8LostDeltaSeconds { get; set; }
    public double AvgAuthorizedL8LostDeltaSeconds { get; set; }
    public double MinAuthorizedL8LostDeltaSeconds { get; set; }
    public double MaxAuthorizedL8LostDeltaSeconds { get; set; }
    public int LastAuthorizedL8LostDeltaHands { get; set; }
    public double AvgAuthorizedL8LostDeltaHands { get; set; }
    public int MinAuthorizedL8LostDeltaHands { get; set; }
    public int MaxAuthorizedL8LostDeltaHands { get; set; }
    public int AuthorizedL8LostDeltaSamples { get; set; }
    public DateTime LastAuthorizedL8LostAtUtc { get; set; }
    public int LastAuthorizedL8LostPBHands { get; set; }
    public DateTime LastL6AuthorizationAtUtc { get; set; }
    public int PBHandsPlayed { get; set; }
    public int LastL6AuthorizationPBHandsPlayed { get; set; }
    public int LastL6AuthorizationScore { get; set; }
    public int LastL6AuthorizationStreak { get; set; }
    public int LastL6AuthorizationShoeHand { get; set; }
    public double LastL6AuthorizationAvgHandSeconds { get; set; }
    public int AuthorizedL8LostFromAuthorizationCount { get; set; }
    public double LastAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public double AvgAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public double MinAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public double MaxAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public int LastAuthorizedL8LossFromAuthorizationHands { get; set; }
    public double LastAuthorizedL8LossSecondsPerHand { get; set; }
    public double AvgAuthorizedL8LossSecondsPerHand { get; set; }
    public double MinAuthorizedL8LossSecondsPerHand { get; set; }
    public double MaxAuthorizedL8LossSecondsPerHand { get; set; }
    public int LastAuthorizedL8LossAuthorizationScore { get; set; }
    public double AvgAuthorizedL8LossAuthorizationScore { get; set; }
    public int CurrentStreak { get; set; }
    public int SecurityRiskScore { get; set; }
    public bool SecurityFilterActive { get; set; }
    public bool PauseBot { get; set; }
    public string PauseScope { get; set; } = "NONE";
    public string PauseComputer { get; set; } = "";
    public int Activations { get; set; }
    public int PreventedL6 { get; set; }
    public int LastShoeHand { get; set; }
    public int Martingala { get; set; }
    public bool HasL6Credit { get; set; }
    public string LastReason { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public int HandSamples { get; set; }
    /// <summary>Valid hand deltas in the rolling avg window (<= 60s, scalping-only).</summary>
    public int ValidSamples { get; set; }
    /// <summary>Deltas discarded because gap exceeded 60s (pause/off/dead time).</summary>
    public int GapFilteredCount { get; set; }
}