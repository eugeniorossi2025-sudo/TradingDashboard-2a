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
}