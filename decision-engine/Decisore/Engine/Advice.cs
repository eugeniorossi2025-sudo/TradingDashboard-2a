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
    public bool   SecurityFilterEnabled    { get; set; } = true;
    public int    SecurityRiskScore       { get; set; }
    public bool   SecurityFilterActive    { get; set; }
    public bool   SecurityFilterPauseBot  { get; set; }
    public string SecurityFilterPauseScope { get; set; } = "NONE";
    public string SecurityFilterPauseComputer { get; set; } = "";
    public double AvgHandSeconds          { get; set; }
    public double LastHandDeltaSeconds    { get; set; }
    public double MinHandDeltaSeconds     { get; set; }
    public double MaxHandDeltaSeconds     { get; set; }
    public int    CurrentStreak           { get; set; }

    // Player Race — filtri 5 e 8 indipendenti dal Security Filter
    public bool   PlayerRace5Triggered      { get; set; }
    public bool   PlayerRace5Ac3Triggered   { get; set; }
    public bool   PlayerRace5PauseBot       { get; set; }
    public bool   PlayerRace8Alert          { get; set; }
    public bool   PlayerRace8Ac3Triggered   { get; set; }
    public bool   PlayerPaceRiskActive      { get; set; }
    public bool   PlayerPaceTriggeredAC3    { get; set; }
    public bool   PlayerPacePauseBot        { get; set; }
    public string PlayerPacePauseScope      { get; set; } = "NONE";
    public string PlayerPacePauseComputer   { get; set; } = "";
}