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
    public double MinHandDeltaSeconds { get; set; }
    public double MaxHandDeltaSeconds { get; set; }
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
}