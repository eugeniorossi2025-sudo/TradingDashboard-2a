using System.Globalization;
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
    private const int OnlineWindowMinutes = 5;

    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<MissionLifecycleService> _logger;

    public MissionLifecycleService(
        AppDbContext context,
        IEmailSender emailSender,
        ILogger<MissionLifecycleService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<MissionLifecycleState> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);
        await EnsureMissionStartedFromFirstPbtAsync(cancellationToken);

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

    public async Task<MissionLifecycleResult> StartCurrentAsync(CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);

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

        var now = DateTime.UtcNow;
        var runtimeMode = await GetCurrentModeAsync(cancellationToken);
        var currentMargin = await GetCurrentMarginAsync(cancellationToken);
        var activeTables = await GetActiveTablesAsync(cancellationToken);
        var target = await GetStopWinTargetAsync(cancellationToken);

        var session = new MissionSession
        {
            MissionKey = $"mission-{now:yyyyMMddHHmmss}",
            StartTime = now,
            TotalMargin = currentMargin,
            LastTotalMarginForRealHands = currentMargin,
            GlobalTarget = target,
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
            Timestamp = now,
            TotalMargin = currentMargin,
            ActiveTables = activeTables,
            VmCurrent = currentMargin,
            RuntimeMode = runtimeMode
        });
        await _context.SaveChangesAsync(cancellationToken);

        var emailSent = await SendMissionEmailAsync(session.Id, "started", cancellationToken);

        return new MissionLifecycleResult
        {
            Success = true,
            Message = "Missione avviata",
            MissionStarted = true,
            MissionSessionId = session.Id,
            EmailSent = emailSent,
            Mission = await GetCurrentAsync(cancellationToken)
        };
    }

    public async Task<MissionLifecycleResult> FinalizeCurrentAsync(string reason, CancellationToken cancellationToken = default)
    {
        await EnsureMissionReportSchemaAsync(cancellationToken);

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

        var emailSent = await SendMissionEmailAsync(session.Id, "finalized", cancellationToken);

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

    public async Task<int> SendMissionEmailAsync(int sessionId, string eventType, CancellationToken cancellationToken = default)
    {
        var session = await _context.MissionSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == sessionId, cancellationToken);

        if (session == null)
            return 0;

        var recipients = await GetMissionRecipientsAsync(cancellationToken);
        if (recipients.Count == 0)
            return 0;

        var isStart = string.Equals(eventType, "started", StringComparison.OrdinalIgnoreCase);
        var subject = isStart
            ? $"DASH2A - Missione avviata #{session.Id} ({session.RuntimeMode})"
            : $"DASH2A - Report missione #{session.Id} ({session.RuntimeMode})";
        var body = BuildMissionEmailBody(session, isStart);

        var sent = 0;
        foreach (var recipient in recipients)
        {
            try
            {
                await _emailSender.SendAsync(recipient, subject, body);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mission email failed for {Recipient}", recipient);
            }
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
        return string.Join(Environment.NewLine, new[]
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
            $"Motivo chiusura: {session.FinalizationReason ?? "-"}",
            "",
            "Apri la pagina Log per visualizzare HTML / JSON / CSV del report:",
            "https://eugenio-dashboard-2a.web.app/pages/log"
        });
    }

    private async Task<string> GetCurrentModeAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations
            .Where(c => c.Key == RuntimeModeKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(value, Demo, StringComparison.OrdinalIgnoreCase) ? Demo : Production;
    }

    private async Task EnsureMissionStartedFromFirstPbtAsync(CancellationToken cancellationToken)
    {
        var hasOpenMission = await _context.MissionSessions
            .AsNoTracking()
            .AnyAsync(session => !session.Completed, cancellationToken);
        if (hasOpenMission)
            return;

        var telemetry = await GetLatestPbtTelemetryAsync(cancellationToken);
        if (telemetry == null || telemetry.TotalPbHandsPlayed <= 0)
            return;

        var alreadyTracked = await _context.MissionSessions
            .AsNoTracking()
            .AnyAsync(session => session.StartTime >= telemetry.SessionStart, cancellationToken);
        if (alreadyTracked)
            return;

        await StartMissionFromPbtAsync(telemetry, cancellationToken);
    }

    private async Task StartMissionFromPbtAsync(PbtTelemetry telemetry, CancellationToken cancellationToken)
    {
        var startPoint = await _context.Margini
            .AsNoTracking()
            .Where(point => point.Data.HasValue && point.Data.Value >= telemetry.SessionStart)
            .OrderBy(point => point.Data)
            .Select(point => new { Timestamp = point.Data!.Value, Margin = point.MargineValue ?? 0m })
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var startTime = startPoint?.Timestamp ?? now;
        var startMargin = startPoint?.Margin ?? await GetCurrentMarginAsync(cancellationToken);
        var runtimeMode = await GetCurrentModeAsync(cancellationToken);
        var activeTables = await GetActiveTablesAsync(cancellationToken);

        var session = new MissionSession
        {
            MissionKey = $"pbt-{telemetry.SessionStart:yyyyMMddHHmmss}",
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

        await SendMissionEmailAsync(session.Id, "started", cancellationToken);
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
