namespace WebApi.Services;

public interface IMissionLifecycleService
{
    Task<MissionLifecycleState> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<MissionLifecycleResult> StartCurrentAsync(CancellationToken cancellationToken = default);
    Task<MissionLifecycleResult> FinalizeCurrentAsync(string reason, CancellationToken cancellationToken = default);
    Task<int> SendMissionEmailAsync(int sessionId, string eventType, CancellationToken cancellationToken = default);
}

public class MissionLifecycleState
{
    public bool HasOpenMission { get; set; }
    public int? SessionId { get; set; }
    public string RuntimeMode { get; set; } = "Production";
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal CurrentMargin { get; set; }
    public decimal TotalMargin { get; set; }
    public decimal GlobalTarget { get; set; }
    public int ActiveTables { get; set; }
    public int RealHandsCount { get; set; }
    public int SamplesCount { get; set; }
    public bool Completed { get; set; }
    public string? FinalizationReason { get; set; }
}

public class MissionLifecycleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool MissionStarted { get; set; }
    public bool MissionFinalized { get; set; }
    public int? MissionSessionId { get; set; }
    public int EmailSent { get; set; }
    public MissionLifecycleState? Mission { get; set; }
}
