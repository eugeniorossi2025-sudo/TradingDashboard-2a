namespace WebApi.Services;

using System.Text.Json.Serialization;

public interface IMissionLifecycleService
{
    Task<MissionLifecycleState> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<MissionLifecycleResult> StartCurrentAsync(CancellationToken cancellationToken = default);
    Task<MissionLifecycleResult> FinalizeCurrentAsync(string reason, CancellationToken cancellationToken = default);
    Task<MissionLifecycleResult?> ObserveLiveStateAsync(CancellationToken cancellationToken = default);
    Task RecordResetBoundaryAsync(CancellationToken cancellationToken = default);
    Task<MissionResetBoundaryState> GetResetBoundaryStateAsync(CancellationToken cancellationToken = default);
    Task<int> SendMissionEmailAsync(int sessionId, string eventType, CancellationToken cancellationToken = default);
    Task<MissionOpenSessionsSnapshot> GetOpenSessionsSnapshotAsync(CancellationToken cancellationToken = default);
    Task<MissionAccountingHealth> GetAccountingHealthAsync(CancellationToken cancellationToken = default);
    Task<MissionRecoveryResult> RecoverMultipleOpenSessionsAsync(CancellationToken cancellationToken = default);
    Task EnsureAccountingInvariantAtStartupAsync(CancellationToken cancellationToken = default);
}

public sealed class MissionOpenSessionsSnapshot
{
    public int Count { get; set; }
    public IReadOnlyList<int> SessionIds { get; set; } = Array.Empty<int>();
    public int? CanonicalSessionId { get; set; }
}

public sealed class MissionAccountingHealth
{
    public bool Healthy => MultipleOpenSessions.Count <= 1;

    [JsonPropertyName("MULTIPLE_OPEN_SESSIONS")]
    public MissionOpenSessionsCheck MultipleOpenSessions { get; set; } = new();
}

public sealed class MissionOpenSessionsCheck
{
    public int Count { get; set; }
    public IReadOnlyList<int> SessionIds { get; set; } = Array.Empty<int>();
}

public sealed class MissionResetBoundaryState
{
    public string? MissionSuppressStartUntilReset { get; set; }
    public bool MissionStartSuppressed { get; set; }
    public DateTime? MissionLastResetAtUtc { get; set; }
}

public sealed class MissionRecoveryResult
{
    public bool RecoveryPerformed { get; set; }
    public int OpenCountBefore { get; set; }
    public IReadOnlyList<int> FinalizedSessionIds { get; set; } = Array.Empty<int>();
    public int? KeptSessionId { get; set; }
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
