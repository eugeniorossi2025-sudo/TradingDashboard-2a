using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Services.Implementations;

public class MissionLifecycleService : IMissionLifecycleService
{
    private const string Production = "Production";
    private const string Demo = "Demo";
    private const string RuntimeModeKey = "RUNTIME_MODE";
    private const string StopWinKey = "STOP_WIN";
    private const string MissionLastResetAtKey = "MISSION_LAST_RESET_AT_UTC";
    private const string MissionSuppressStartUntilResetKey = "MISSION_SUPPRESS_START_UNTIL_RESET";
    private const string MissionAccountingRecoveryAtKey = "MISSION_ACCOUNTING_RECOVERY_AT_UTC";
    private const string MultipleOpenRecoveryReason = "MultipleOpenRecovery";
    private const int OnlineWindowMinutes = 5;

    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IMissionReportBuilder _missionReportBuilder;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<MissionLifecycleService> _logger;

    public MissionLifecycleService(
        AppDbContext context,
        IEmailSender emailSender,
        IMissionReportBuilder missionReportBuilder,
        IPushNotificationService pushNotificationService,
        ILogger<MissionLifecycleService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _missionReportBuilder = missionReportBuilder;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<MissionLifecycleState> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);

        var open = await _context.MissionSessions
            .AsNoTracking()
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (open == null)
        {
            return new MissionLifecycleState
            {
                RuntimeMode = await GetCurrentModeAsync(cancellationToken),
                CurrentMargin = await GetCurrentMarginAsync(cancellationToken),
                ActiveTables = await GetActiveTablesAsync(cancellationToken)
            };
        }

        var samplesCount = await _context.MissionMarginSamples
            .AsNoTracking()
            .CountAsync(sample => sample.SessionId == open.Id, cancellationToken);

        return new MissionLifecycleState
        {
            HasOpenMission = true,
            SessionId = open.Id,
            RuntimeMode = open.RuntimeMode,
            StartTime = open.StartTime,
            EndTime = open.EndTime,
            CurrentMargin = await GetCurrentMarginAsync(cancellationToken),
            TotalMargin = open.TotalMargin,
            GlobalTarget = open.GlobalTarget,
            ActiveTables = open.ActiveTables,
            RealHandsCount = open.RealHandsCount,
            SamplesCount = samplesCount,
            Completed = open.Completed,
            FinalizationReason = open.FinalizationReason
        };
    }

    public async Task<MissionOpenSessionsSnapshot> GetOpenSessionsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);
        var ids = await _context.MissionSessions
            .AsNoTracking()
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .Select(session => session.Id)
            .ToListAsync(cancellationToken);

        return new MissionOpenSessionsSnapshot
        {
            Count = ids.Count,
            SessionIds = ids,
            CanonicalSessionId = ids.Count > 0 ? ids[0] : null
        };
    }

    public async Task<MissionAccountingHealth> GetAccountingHealthAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetOpenSessionsSnapshotAsync(cancellationToken);
        return new MissionAccountingHealth
        {
            MultipleOpenSessions = new MissionOpenSessionsCheck
            {
                Count = snapshot.Count,
                SessionIds = snapshot.SessionIds
            }
        };
    }

    public async Task<MissionRecoveryResult> RecoverMultipleOpenSessionsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);

        var openSessions = await _context.MissionSessions
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .ToListAsync(cancellationToken);

        if (openSessions.Count <= 1)
        {
            return new MissionRecoveryResult
            {
                OpenCountBefore = openSessions.Count,
                KeptSessionId = openSessions.FirstOrDefault()?.Id
            };
        }

        var ids = openSessions.Select(session => session.Id).ToList();
        _logger.LogCritical(
            "MULTIPLE_OPEN_SESSIONS detected: count={Count} ids=[{Ids}]. Finalizing stale sessions, keeping #{KeepId}.",
            openSessions.Count,
            string.Join(",", ids),
            openSessions[0].Id);

        var finalizedIds = new List<int>();
        foreach (var stale in openSessions.Skip(1))
        {
            await FinalizeSessionAsync(stale, MultipleOpenRecoveryReason, cancellationToken, sendMissionEmail: false);
            finalizedIds.Add(stale.Id);
        }

        await RecordRecoveryEventAsync(openSessions.Count, finalizedIds, openSessions[0].Id, cancellationToken);

        return new MissionRecoveryResult
        {
            RecoveryPerformed = true,
            OpenCountBefore = openSessions.Count,
            FinalizedSessionIds = finalizedIds,
            KeptSessionId = openSessions[0].Id
        };
    }

    public async Task EnsureAccountingInvariantAtStartupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mission accounting startup invariant check starting");
        var recovery = await RecoverMultipleOpenSessionsAsync(cancellationToken);
        if (recovery.RecoveryPerformed)
        {
            _logger.LogWarning(
                "Mission accounting startup recovery completed: finalized [{FinalizedIds}], kept #{KeptId}",
                string.Join(",", recovery.FinalizedSessionIds),
                recovery.KeptSessionId);
        }
    }

    public async Task<MissionLifecycleResult?> ObserveLiveStateAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);
        await RecoverMultipleOpenSessionsAsync(cancellationToken);

        var open = await _context.MissionSessions
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (open != null)
        {
            if (await HasStopWinActionCodeAsync(open, cancellationToken))
            {
                var result = await FinalizeCurrentAsync("ActionCode1_STOP_WIN", cancellationToken);
                await SetConfigurationValueAsync(MissionSuppressStartUntilResetKey, "1", "Mission start is suppressed after AC1 close until the next dashboard reset.", cancellationToken);
                return result;
            }

            return null;
        }

        if (await IsMissionStartSuppressedAsync(cancellationToken))
            return null;

        var resetAt = await GetLastResetBoundaryAsync(cancellationToken);
        if (!resetAt.HasValue)
            return null;

        var firstPoint = await _context.Margini
            .AsNoTracking()
            .Where(point => point.Data.HasValue && point.Data.Value > resetAt.Value)
            .OrderBy(point => point.Data)
            .Select(point => new { Timestamp = point.Data!.Value, Margin = point.MargineValue ?? 0m })
            .FirstOrDefaultAsync(cancellationToken);

        if (firstPoint == null)
            return null;

        var alreadyTracked = await _context.MissionSessions
            .AsNoTracking()
            .AnyAsync(session =>
                session.StartTime >= firstPoint.Timestamp
                && (session.MissionKey == null
                    || (!session.MissionKey.StartsWith("historical-demo-import:")
                        && session.FinalizationReason != "HistoricalImport")),
                cancellationToken);
        if (alreadyTracked)
            return null;

        return await StartMissionFromFirstMarginPointAsync(firstPoint.Timestamp, firstPoint.Margin, cancellationToken);
    }

    public async Task RecordResetBoundaryAsync(CancellationToken cancellationToken = default)
    {
        await SetConfigurationValueAsync(MissionLastResetAtKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture), "Last dashboard reset accepted by Decisore; mission starts only from the next real PBT/margin point.", cancellationToken);
        await SetConfigurationValueAsync(MissionSuppressStartUntilResetKey, "0", "Mission start is suppressed after AC1 close until the next dashboard reset.", cancellationToken);
    }

    public async Task<MissionLifecycleResult> StartCurrentAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);
        await RecoverMultipleOpenSessionsAsync(cancellationToken);

        var existing = await _context.MissionSessions
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            return new MissionLifecycleResult
            {
                Success = false,
                Message = "Esiste gia una missione aperta",
                MissionSessionId = existing.Id,
                Mission = await GetCurrentAsync(cancellationToken)
            };
        }

        var observed = await ObserveLiveStateAsync(cancellationToken);
        if (observed?.MissionStarted == true)
            return observed;

        return new MissionLifecycleResult
        {
            Success = false,
            Message = "La missione parte solo dal primo PBT reale successivo al reset dashboard",
            Mission = await GetCurrentAsync(cancellationToken)
        };
    }

    public async Task<MissionLifecycleResult> FinalizeCurrentAsync(string reason, CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);
        await RecoverMultipleOpenSessionsAsync(cancellationToken);

        var session = await _context.MissionSessions
            .Where(row => !row.Completed)
            .OrderByDescending(row => row.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (session == null)
        {
            return new MissionLifecycleResult
            {
                Success = true,
                Message = "Nessuna missione aperta da finalizzare",
                MissionFinalized = false,
                Mission = await GetCurrentAsync(cancellationToken)
            };
        }

        return await FinalizeSessionAsync(session, reason, cancellationToken);
    }

    public async Task<int> SendMissionEmailAsync(int sessionId, string eventType, CancellationToken cancellationToken = default)
    {
        var session = await _context.MissionSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == sessionId, cancellationToken);

        if (session == null)
            return 0;

        var recipients = await GetMissionRecipientsAsync(cancellationToken);
        var isStart = string.Equals(eventType, "started", StringComparison.OrdinalIgnoreCase);
        var subject = isStart
            ? $"DASH2A - Missione avviata #{session.Id} ({session.RuntimeMode})"
            : $"DASH2A - Report missione #{session.Id} ({session.RuntimeMode})";
        var body = BuildMissionEmailBody(session, isStart);
        IReadOnlyList<EmailAttachment>? attachments = null;

        if (!isStart)
        {
            try
            {
                var reportHtml = await _missionReportBuilder.BuildSessionReportHtmlAsync(sessionId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(reportHtml))
                {
                    attachments = new[]
                    {
                        new EmailAttachment(
                            BuildMissionReportAttachmentFileName(session),
                            "text/html; charset=utf-8",
                            Encoding.UTF8.GetBytes(reportHtml))
                    };
                }
                else
                {
                    _logger.LogWarning("Mission report HTML empty for session {SessionId}; email sent without attachment", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mission report attachment skipped for session {SessionId}", sessionId);
            }
        }

        var sent = 0;
        foreach (var recipient in recipients)
        {
            try
            {
                await _emailSender.SendAsync(recipient, subject, body, attachments);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mission email failed for {Recipient}", recipient);
            }
        }

        try
        {
            await _pushNotificationService.SendMissionNotificationAsync(session.Id, eventType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mission push notification failed for session {SessionId}", session.Id);
        }

        return sent;
    }

    private async Task<List<string>> GetMissionRecipientsAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.UserNotificationSettings
            .AsNoTracking()
            .Where(setting => setting.Enabled && setting.Mission)
            .Select(setting => new
            {
                setting.NotificationEmail,
                LoginEmail = setting.User == null ? null : setting.User.Email
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => string.IsNullOrWhiteSpace(row.NotificationEmail) ? row.LoginEmail : row.NotificationEmail)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildMissionEmailBody(MissionSession session, bool isStart)
    {
        var end = session.EndTime.HasValue ? session.EndTime.Value.ToString("O", CultureInfo.InvariantCulture) : "-";
        var lines = new List<string>
        {
            isStart ? "Missione DASH2A avviata." : "Missione DASH2A finalizzata.",
            "",
            $"Sessione: #{session.Id}",
            $"Runtime: {session.RuntimeMode}",
            $"Start UTC: {session.StartTime:O}",
            $"End UTC: {end}",
            $"Margine: {session.TotalMargin:0.00} EUR",
            $"Target: {session.GlobalTarget:0.00} EUR",
            $"Tavoli attivi: {session.ActiveTables}",
            $"Mani reali: {session.RealHandsCount}",
            $"Motivo chiusura: {session.FinalizationReason ?? "-"}"
        };

        if (!isStart)
        {
            lines.Add("");
            lines.Add("Il report HTML della missione e' allegato a questa email.");
            lines.Add("");
            lines.Add("Puoi aprire anche la pagina Log per altri formati (JSON / CSV):");
            lines.Add("https://eugenio-dashboard-2a.web.app/pages/log");
        }
        else
        {
            lines.Add("");
            lines.Add("Riceverai il report in allegato al termine della missione.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildMissionReportAttachmentFileName(MissionSession session)
    {
        var runtime = string.IsNullOrWhiteSpace(session.RuntimeMode)
            ? "Unknown"
            : session.RuntimeMode.Trim().Replace(' ', '_');
        return $"missione_{session.Id}_{runtime}.html";
    }

    private async Task<string> GetCurrentModeAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations
            .Where(c => c.Key == RuntimeModeKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(value, Demo, StringComparison.OrdinalIgnoreCase) ? Demo : Production;
    }

    private async Task<MissionLifecycleResult?> StartMissionFromFirstMarginPointAsync(DateTime startTime, decimal startMargin, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var openCount = await _context.MissionSessions.CountAsync(session => !session.Completed, cancellationToken);
        if (openCount > 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(
                "Mission start blocked: {OpenCount} open session(s) already exist (invariant: at most one).",
                openCount);
            return null;
        }

        var now = DateTime.UtcNow;
        var runtimeMode = await GetCurrentModeAsync(cancellationToken);
        var activeTables = await GetActiveTablesAsync(cancellationToken);

        var session = new MissionSession
        {
            MissionKey = $"pbt-{startTime:yyyyMMddHHmmss}",
            StartTime = startTime,
            TotalMargin = startMargin,
            LastTotalMarginForRealHands = startMargin,
            GlobalTarget = await GetStopWinTargetAsync(cancellationToken),
            ActiveTables = activeTables,
            RuntimeMode = runtimeMode,
            Completed = false,
            CreatedAt = now
        };

        _context.MissionSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        _context.MissionMarginSamples.Add(new MissionMarginSample
        {
            SessionId = session.Id,
            Timestamp = startTime,
            TotalMargin = startMargin,
            ActiveTables = activeTables,
            VmCurrent = startMargin,
            RuntimeMode = runtimeMode
        });
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var emailSent = await SendMissionEmailAsync(session.Id, "started", cancellationToken);

        return new MissionLifecycleResult
        {
            Success = true,
            Message = "Missione avviata dal primo PBT reale dopo reset dashboard",
            MissionStarted = true,
            MissionSessionId = session.Id,
            EmailSent = emailSent,
            Mission = await GetCurrentAsync(cancellationToken)
        };
    }

    private async Task<MissionLifecycleResult> FinalizeSessionAsync(
        MissionSession session,
        string reason,
        CancellationToken cancellationToken,
        bool sendMissionEmail = true)
    {
        var now = DateTime.UtcNow;
        var activeTables = await GetActiveTablesAsync(cancellationToken);
        var currentMargin = await GetCurrentMarginAsync(cancellationToken);
        var marginPoints = await _context.Margini
            .AsNoTracking()
            .Where(point => point.Data.HasValue && point.Data.Value >= session.StartTime && point.Data.Value <= now)
            .OrderBy(point => point.Data)
            .Select(point => new { Timestamp = point.Data!.Value, Margin = point.MargineValue ?? 0m })
            .ToListAsync(cancellationToken);

        var existingTimestamps = await _context.MissionMarginSamples
            .Where(sample => sample.SessionId == session.Id)
            .Select(sample => sample.Timestamp)
            .ToListAsync(cancellationToken);
        var timestampSet = existingTimestamps.ToHashSet();

        var newSamples = new List<MissionMarginSample>();
        foreach (var point in marginPoints)
        {
            if (timestampSet.Contains(point.Timestamp))
                continue;

            newSamples.Add(new MissionMarginSample
            {
                SessionId = session.Id,
                Timestamp = point.Timestamp,
                TotalMargin = point.Margin,
                ActiveTables = activeTables,
                VmCurrent = point.Margin,
                RuntimeMode = session.RuntimeMode
            });
        }

        if (!timestampSet.Contains(now))
        {
            newSamples.Add(new MissionMarginSample
            {
                SessionId = session.Id,
                Timestamp = now,
                TotalMargin = currentMargin,
                ActiveTables = activeTables,
                VmCurrent = currentMargin,
                RuntimeMode = session.RuntimeMode
            });
        }

        if (newSamples.Count > 0)
            _context.MissionMarginSamples.AddRange(newSamples);

        session.EndTime = now;
        session.TotalMargin = currentMargin;
        session.ActiveTables = activeTables;
        session.RealHandsCount = await GetRealHandsCountAsync(session.StartTime, now, cancellationToken);
        session.Completed = true;
        session.ReportPublishedAt = now;
        session.FinalizationReason = string.IsNullOrWhiteSpace(reason) ? "ManualFinalize" : reason;

        await _context.SaveChangesAsync(cancellationToken);

        var emailSent = 0;
        if (sendMissionEmail)
            emailSent = await SendMissionEmailAsync(session.Id, "finalized", cancellationToken);

        return new MissionLifecycleResult
        {
            Success = true,
            Message = "Missione finalizzata",
            MissionFinalized = true,
            MissionSessionId = session.Id,
            EmailSent = emailSent,
            Mission = new MissionLifecycleState
            {
                HasOpenMission = false,
                SessionId = session.Id,
                RuntimeMode = session.RuntimeMode,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                CurrentMargin = currentMargin,
                TotalMargin = session.TotalMargin,
                GlobalTarget = session.GlobalTarget,
                ActiveTables = session.ActiveTables,
                RealHandsCount = session.RealHandsCount,
                SamplesCount = await _context.MissionMarginSamples.CountAsync(sample => sample.SessionId == session.Id, cancellationToken),
                Completed = true,
                FinalizationReason = session.FinalizationReason
            }
        };
    }

    private async Task RecordRecoveryEventAsync(
        int openCountBefore,
        IReadOnlyList<int> finalizedSessionIds,
        int keptSessionId,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            atUtc = DateTime.UtcNow,
            openCountBefore,
            finalizedSessionIds,
            keptSessionId,
            reason = MultipleOpenRecoveryReason
        });

        await SetConfigurationValueAsync(
            MissionAccountingRecoveryAtKey,
            payload,
            "Last mission accounting recovery for multiple open sessions (MULTIPLE_OPEN_SESSIONS).",
            cancellationToken);
    }

    private async Task<PbtTelemetry?> GetLatestPbtTelemetryAsync(CancellationToken cancellationToken)
    {
        var latest = await _context.Statistiche
            .AsNoTracking()
            .Where(row => row.Telemetry != null)
            .OrderByDescending(row => row.DataInizio)
            .FirstOrDefaultAsync(cancellationToken);

        if (latest?.Telemetry == null)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(latest.Telemetry);
            var totalPbHandsPlayed = GetInt32Property(doc.RootElement, "TotalPBHandsPlayed", "totalPbHandsPlayed");
            return new PbtTelemetry(latest.DataInizio, totalPbHandsPlayed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to parse latest PBT telemetry for mission auto-start");
            return null;
        }
    }

    private static int GetInt32Property(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
                return property.GetInt32();
        }

        return 0;
    }

    private async Task<decimal> GetCurrentMarginAsync(CancellationToken cancellationToken)
    {
        var latestMargin = await _context.Margini
            .AsNoTracking()
            .OrderByDescending(point => point.Data)
            .Select(point => point.MargineValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestMargin.HasValue)
            return latestMargin.Value;

        return await _context.PcCurrentStatuses
            .AsNoTracking()
            .SumAsync(row => row.Margine, cancellationToken);
    }

    private async Task<int> GetActiveTablesAsync(CancellationToken cancellationToken)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-OnlineWindowMinutes);
        return await _context.PcCurrentStatuses
            .AsNoTracking()
            .CountAsync(row => row.LastUpdate >= threshold, cancellationToken);
    }

    private async Task<decimal> GetStopWinTargetAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations
            .Where(c => c.Key == StopWinKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return decimal.TryParse(value?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var target)
            ? target
            : 0m;
    }

    private async Task<bool> HasStopWinActionCodeAsync(MissionSession open, CancellationToken cancellationToken)
    {
        var rows = await _context.PcCurrentStatuses
            .AsNoTracking()
            .Where(row => row.LastUpdate >= open.StartTime && row.LastAdvice != null)
            .Select(row => row.LastAdvice)
            .ToListAsync(cancellationToken);

        foreach (var lastAdvice in rows)
        {
            var action = TryGetInt32Property(lastAdvice, "ActionCode");
            if (action != 1)
                continue;

            var reason = TryGetStringProperty(lastAdvice, "Reason");
            if (string.Equals(reason, "STOP_WIN", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private async Task<bool> IsMissionStartSuppressedAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations
            .AsNoTracking()
            .Where(row => row.Key == MissionSuppressStartUntilResetKey)
            .Select(row => row.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<DateTime?> GetLastResetBoundaryAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations
            .AsNoTracking()
            .Where(row => row.Key == MissionLastResetAtKey)
            .Select(row => row.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }

    private async Task SetConfigurationValueAsync(string key, string value, string description, CancellationToken cancellationToken)
    {
        var configuration = await _context.Configurations
            .FirstOrDefaultAsync(row => row.Key == key, cancellationToken);

        if (configuration == null)
        {
            _context.Configurations.Add(new Configuration
            {
                Key = key,
                Value = value,
                Description = description,
                Pos = 990
            });
        }
        else
        {
            configuration.Value = value;
            configuration.Description ??= description;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static int? TryGetInt32Property(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
                ? property.GetInt32()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryGetStringProperty(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<int> GetRealHandsCountAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var latestTelemetry = await _context.Statistiche
            .AsNoTracking()
            .Where(row => row.DataInizio >= start && row.DataInizio <= end && row.Telemetry != null)
            .OrderByDescending(row => row.DataInizio)
            .Select(row => row.Telemetry)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(latestTelemetry))
        {
            try
            {
                using var doc = JsonDocument.Parse(latestTelemetry);
                if (doc.RootElement.TryGetProperty("TotalPBHandsPlayed", out var totalPbHands))
                    return totalPbHands.GetInt32();
            }
            catch
            {
                // Fall back to margin moves below.
            }
        }

        var margins = await _context.Margini
            .AsNoTracking()
            .Where(point => point.Data.HasValue && point.Data.Value >= start && point.Data.Value <= end)
            .OrderBy(point => point.Data)
            .Select(point => point.MargineValue ?? 0m)
            .ToListAsync(cancellationToken);

        return CountMarginMoves(margins);
    }

    private static int CountMarginMoves(IEnumerable<decimal> margins)
    {
        decimal? previous = null;
        var count = 0;
        foreach (var margin in margins)
        {
            if (previous.HasValue && margin != previous.Value)
                count++;
            previous = margin;
        }

        return count;
    }

    private async Task EnsureMissionReportSchemaAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[MissionSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionSessions] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionSessions] PRIMARY KEY,
        [MissionKey] nvarchar(128) NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [RealHandsCount] int NOT NULL,
        [LastTotalMarginForRealHands] decimal(18,2) NULL,
        [GlobalTarget] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [KFactor] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        [Completed] bit NOT NULL,
        [ReportPublishedAt] datetime2 NULL,
        [FinalizationReason] nvarchar(128) NULL,
        [CreatedAt] datetime2 NOT NULL
    );
END

IF OBJECT_ID(N'[dbo].[MissionMarginSamples]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionMarginSamples] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionMarginSamples] PRIMARY KEY,
        [SessionId] int NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [VmCurrent] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        CONSTRAINT [FK_MissionMarginSamples_MissionSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[MissionSessions]([ID]) ON DELETE CASCADE
    );
END
""", cancellationToken);
    }
}

internal sealed record PbtTelemetry(DateTime SessionStart, int TotalPbHandsPlayed);
