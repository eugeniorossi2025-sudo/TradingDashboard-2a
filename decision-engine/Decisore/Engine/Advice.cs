namespace Decisore.Engine;

public class Advice
{
    public int TableId { get; set; }
    public string State { get; set; }
    public int Martingala { get; set; }
    public double LocalMargin { get; set; }
    public double GlobalMargin { get; set; }
    public double Elapsed { get; set; }
    public bool HotZone { get; set; }
    public string HotZoneLabel { get; set; }
    public bool StopL6 { get; set; }
    public int GlobalAuthL6Counter { get; set; }
    public int GlobalL5Loss { get; set; }
    public int GlobalPBHandsPlayed { get; set; }
    public bool GlobalPauseScalping { get; set; }
    public int GlobalPauseScalpingDuration { get; set; }
    public string Reason { get; set; }
    public string ToolTipJson { get; set; }
    public bool StopMission { get; set; }
    public int ActionCode { get; set; }

    // Security Filter — compressione temporale streak inizio shoe
    public int    SecurityRiskScore       { get; set; }
    public bool   SecurityFilterActive    { get; set; }
    public double AvgHandSeconds          { get; set; }
    public double LastHandDeltaSeconds    { get; set; }
    public int    CurrentStreak           { get; set; }
}