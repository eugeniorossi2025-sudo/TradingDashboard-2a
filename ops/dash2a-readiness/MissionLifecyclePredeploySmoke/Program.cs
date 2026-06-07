using System.Globalization;
using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Data;
using WebApi.Services;
using WebApi.Services.Implementations;

const string DbName = "Dash2A_LifecycleSmoke_Val";
var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={DbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
var runner = new ValidationRunner(connectionString);
var exitCode = await runner.RunAsync();
Environment.Exit(exitCode);

sealed class ValidationRunner
{
    private const string DbName = "Dash2A_LifecycleSmoke_Val";
    private readonly string _connectionString;
    private readonly List<(string Id, string Label, bool Pass, string Evidence)> _results = new();

    public ValidationRunner(string connectionString) => _connectionString = connectionString;

    public async Task<int> RunAsync()
    {
        try
        {
            await PrepareDatabaseAsync();
            await RunPoint1UniqueIndexAsync();
            await RunPoint2OpenCasesAsync();
            await RunPoint3StopWinAsync();
            await RunPoint4ResetAsync();
            await RunPoint5CompatibilityAsync();
        }
        catch (Exception ex)
        {
            Record("FATAL", "Smoke harness", false, ex.ToString());
        }

        PrintReport();
        return _results.Any(r => !r.Pass) ? 1 : 0;
    }

    private void Record(string id, string label, bool pass, string evidence) =>
        _results.Add((id, label, pass, evidence));

    private async Task PrepareDatabaseAsync()
    {
        var master = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" }.ConnectionString;
        await using (var conn = new SqlConnection(master))
        {
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"""
                IF DB_ID(N'{DbName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{DbName}];
                END;
                CREATE DATABASE [{DbName}];
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        await using var setup = db.CreateCommand();
        setup.CommandText = """
            CREATE TABLE [dbo].[Configurations] (
                [K] nvarchar(50) NOT NULL PRIMARY KEY,
                [Description] nvarchar(500) NULL,
                [Pos] int NULL,
                [Value] nvarchar(4000) NULL
            );

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

            CREATE TABLE [dbo].[Margini] (
                [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Margine] decimal(18,2) NULL,
                [Data] datetime2 NULL
            );

            CREATE TABLE [dbo].[Statistiche] (
                [ID] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [DATA_INIZIO] datetime2 NOT NULL,
                [Telemetry] nvarchar(max) NULL
            );

            CREATE TABLE [dbo].[Users_v2] (
                [Id] int NOT NULL PRIMARY KEY,
                [UserName] nvarchar(256) NULL,
                [NormalizedUserName] nvarchar(256) NULL,
                [Email] nvarchar(256) NULL,
                [NormalizedEmail] nvarchar(256) NULL,
                [EmailConfirmed] bit NOT NULL DEFAULT 0,
                [PasswordHash] nvarchar(max) NULL,
                [SecurityStamp] nvarchar(max) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                [PhoneNumber] nvarchar(max) NULL,
                [PhoneNumberConfirmed] bit NOT NULL DEFAULT 0,
                [TwoFactorEnabled] bit NOT NULL DEFAULT 0,
                [LockoutEnd] datetimeoffset NULL,
                [LockoutEnabled] bit NOT NULL DEFAULT 0,
                [AccessFailedCount] int NOT NULL DEFAULT 0
            );

            CREATE TABLE [dbo].[UserNotificationSettings] (
                [ID] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [UserId] int NOT NULL,
                [Enabled] bit NOT NULL DEFAULT 0,
                [Mission] bit NOT NULL DEFAULT 0,
                [System] bit NOT NULL DEFAULT 0,
                [Errors] bit NOT NULL DEFAULT 0,
                [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
                [UpdatedAtUtc] datetime2 NULL,
                [NotificationEmail] nvarchar(256) NULL
            );

            CREATE TABLE [dbo].[Pc_CurrentStatus] (
                [COMPUTER] nvarchar(50) NOT NULL PRIMARY KEY,
                [KEY_ULTIMO] decimal(18,2) NOT NULL DEFAULT 0,
                [DT_ULTIMO] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
                [ACCOUNT] nvarchar(500) NULL,
                [TAVOLO] nvarchar(500) NULL,
                [SALDO_INIZIALE] decimal(18,2) NOT NULL DEFAULT 0,
                [SALDO_ISTANTANEO] decimal(18,2) NOT NULL DEFAULT 0,
                [MARGINE] decimal(18,2) NOT NULL DEFAULT 0,
                [MEDIA_ORA] decimal(18,2) NOT NULL DEFAULT 0,
                [VALORE_GIOCATO] decimal(18,2) NOT NULL DEFAULT 0,
                [COLPO_MARTINGALA] int NOT NULL DEFAULT 0,
                [STATO] nvarchar(100) NULL,
                [COLORE] nvarchar(20) NULL,
                [CHOSEN_COLOR] nvarchar(1) NULL,
                [MAZZO] nvarchar(50) NULL,
                [PBT] nvarchar(1) NULL,
                [ORE] decimal(18,2) NOT NULL DEFAULT 0,
                [LAST_UPDATE] datetime2 NOT NULL,
                [LAST_ADVICE] nvarchar(4000) NULL,
                [LAST_INFO] nvarchar(4000) NULL,
                [VALUTAZIONE_RISULTATO] nvarchar(4000) NULL
            );
            """;
        await setup.ExecuteNonQueryAsync();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDbContext(options);
    }

    private static MissionLifecycleService CreateService(AppDbContext context)
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<MissionLifecycleService>();
        return new MissionLifecycleService(
            context,
            new NoopEmailSender(),
            new MissionReportBuilder(context),
            new NoopPushNotificationService(),
            logger);
    }

    private static async Task SeedBaseConfigAsync(AppDbContext ctx, DateTime resetAtUtc)
    {
        ctx.Configurations.AddRange(
            new Configuration { Key = "RUNTIME_MODE", Value = "Production", Pos = 1 },
            new Configuration { Key = "STOP_WIN", Value = "100", Pos = 2 },
            new Configuration { Key = "MISSION_LAST_RESET_AT_UTC", Value = resetAtUtc.ToString("O", CultureInfo.InvariantCulture), Pos = 990 },
            new Configuration { Key = "MISSION_SUPPRESS_START_UNTIL_RESET", Value = "0", Pos = 991 });
        await ctx.SaveChangesAsync();
    }

    private static async Task ClearMissionDataAsync(AppDbContext ctx)
    {
        ctx.MissionMarginSamples.RemoveRange(ctx.MissionMarginSamples);
        ctx.MissionSessions.RemoveRange(ctx.MissionSessions);
        ctx.Margini.RemoveRange(ctx.Margini);
        ctx.PcCurrentStatuses.RemoveRange(ctx.PcCurrentStatuses);
        await ctx.SaveChangesAsync();
    }

    private async Task<int> ScalarIntAsync(string sql)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var value = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(value);
    }

    private async Task RunPoint1UniqueIndexAsync()
    {
        await using var ctx = CreateContext();
        var service = CreateService(ctx);
        _ = await service.GetOpenSessionsSnapshotAsync();

        var openBefore = await ScalarIntAsync("SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0;");
        var indexExists = await ScalarIntAsync("""
            SELECT COUNT(*) FROM sys.indexes
            WHERE name = N'UX_MissionSessions_OneOpen' AND object_id = OBJECT_ID(N'dbo.MissionSessions');
            """);

        Record("1a", "OPEN_SESSION_COUNT pre-index (must be <=1)", openBefore <= 1,
            $"COUNT(Completed=0)={openBefore}");

        Record("1b", "UX_MissionSessions_OneOpen created by EnsureMissionReportSchema", indexExists == 1,
            $"sys.indexes hit={indexExists}");

        ctx.MissionSessions.Add(new MissionSession
        {
            MissionKey = "smoke-open-1",
            StartTime = DateTime.UtcNow,
            TotalMargin = 0,
            GlobalTarget = 100,
            ActiveTables = 1,
            RuntimeMode = "Production",
            Completed = false,
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var duplicateBlocked = false;
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO dbo.MissionSessions
                (MissionKey, StartTime, TotalMargin, RealHandsCount, GlobalTarget, ActiveTables, KFactor, RuntimeMode, Completed, CreatedAt)
                VALUES (N'smoke-open-2', SYSUTCDATETIME(), 0, 0, 100, 1, 1, N'Production', 0, SYSUTCDATETIME());
                """;
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            duplicateBlocked = true;
        }

        var openAfter = await ScalarIntAsync("SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0;");
        Record("1c", "Second open row blocked by unique index", duplicateBlocked && openAfter == 1,
            $"duplicateBlocked={duplicateBlocked}, openCount={openAfter}");

        var health = await service.GetAccountingHealthAsync();
        var current = await service.GetCurrentAsync();
        Record("1d", "GetAccountingHealth + GetCurrentAsync after index", health.MultipleOpenSessions.Count <= 1 && current.HasOpenMission,
            $"healthOpenCount={health.MultipleOpenSessions.Count}, hasOpen={current.HasOpenMission}");

        var reportCount = await ScalarIntAsync("SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0;");
        var sampleCount = await ScalarIntAsync("SELECT COUNT(*) FROM dbo.MissionMarginSamples s INNER JOIN dbo.MissionSessions m ON m.ID = s.SessionId WHERE m.Completed = 0;");
        Record("1e", "Typical read queries (open session + samples join)", reportCount == 1 && sampleCount >= 0,
            $"openSessions={reportCount}, openSamples={sampleCount}");
    }

    private async Task RunPoint2OpenCasesAsync()
    {
        var resetAt = DateTime.UtcNow.AddMinutes(-30);

        await using (var ctx = CreateContext())
        {
            await ClearMissionDataAsync(ctx);
            ctx.Configurations.RemoveRange(ctx.Configurations);
            await ctx.SaveChangesAsync();
            await SeedBaseConfigAsync(ctx, resetAt);

            ctx.Margini.Add(new Margine { MargineValue = 12m, Data = resetAt.AddMinutes(5) });
            await ctx.SaveChangesAsync();

            var service = CreateService(ctx);
            var observed = await service.ObserveLiveStateAsync();
            var openCount = await ctx.MissionSessions.CountAsync(s => !s.Completed);

            Record("2A", "Reset + Margini senza decide → missione NON aperta",
                observed?.MissionStarted != true && openCount == 0,
                $"MissionStarted={observed?.MissionStarted}, openCount={openCount}");
        }

        await using (var ctx = CreateContext())
        {
            await ClearMissionDataAsync(ctx);
            ctx.Configurations.RemoveRange(ctx.Configurations);
            await ctx.SaveChangesAsync();
            await SeedBaseConfigAsync(ctx, resetAt);

            var decideAt = DateTime.UtcNow.AddSeconds(-15);
            ctx.PcCurrentStatuses.Add(new PcCurrentStatus
            {
                Computer = "SMOKE-PC1",
                LastUpdate = decideAt,
                LastAdvice = """{"ActionCode":0,"Reason":"PLAY"}""",
                Margine = 5m,
                DtUltimo = decideAt
            });
            ctx.Margini.Add(new Margine { MargineValue = 15m, Data = decideAt.AddSeconds(5) });
            await ctx.SaveChangesAsync();

            var service = CreateService(ctx);
            var observed = await service.ObserveLiveStateAsync();
            var open = await ctx.MissionSessions.FirstOrDefaultAsync(s => !s.Completed);

            Record("2B", "Reset + decide + ActiveTables>=1 + Margini → missione aperta",
                observed?.MissionStarted == true && open != null,
                $"MissionStarted={observed?.MissionStarted}, sessionId={open?.Id}, start={open?.StartTime:o}");
        }
    }

    private async Task RunPoint3StopWinAsync()
    {
        await using var ctx = CreateContext();
        await ClearMissionDataAsync(ctx);
        ctx.Configurations.RemoveRange(ctx.Configurations);
        await ctx.SaveChangesAsync();
        await SeedBaseConfigAsync(ctx, DateTime.UtcNow.AddHours(-2));

        var start = DateTime.UtcNow.AddHours(-1);
        ctx.MissionSessions.Add(new MissionSession
        {
            MissionKey = "smoke-stopwin",
            StartTime = start,
            TotalMargin = 90m,
            GlobalTarget = 100m,
            ActiveTables = 1,
            RuntimeMode = "Production",
            Completed = false,
            CreatedAt = start
        });
        ctx.Margini.Add(new Margine { MargineValue = 120m, Data = DateTime.UtcNow });
        ctx.PcCurrentStatuses.Add(new PcCurrentStatus
        {
            Computer = "SMOKE-PC2",
            LastUpdate = DateTime.UtcNow,
            LastAdvice = """{"ActionCode":0,"Reason":"PLAY"}""",
            Margine = 120m,
            DtUltimo = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var observed = await service.ObserveLiveStateAsync();
        var session = await ctx.MissionSessions.OrderByDescending(s => s.Id).FirstAsync();
        var suppress = await ctx.Configurations.Where(c => c.Key == "MISSION_SUPPRESS_START_UNTIL_RESET").Select(c => c.Value).FirstAsync();

        Record("3", "Margine >= GlobalTarget senza AC1 → StopWinMarginThreshold + suppress",
            observed?.MissionFinalized == true
            && session.Completed
            && session.FinalizationReason == "StopWinMarginThreshold"
            && suppress == "1",
            $"finalized={observed?.MissionFinalized}, reason={session.FinalizationReason}, suppress={suppress}, totalMargin={session.TotalMargin}");
    }

    private async Task RunPoint4ResetAsync()
    {
        await using var ctx = CreateContext();
        await ClearMissionDataAsync(ctx);
        ctx.Configurations.RemoveRange(ctx.Configurations);
        await ctx.SaveChangesAsync();

        var start = DateTime.UtcNow.AddHours(-1);
        ctx.MissionSessions.Add(new MissionSession
        {
            MissionKey = "smoke-reset",
            StartTime = start,
            TotalMargin = 40m,
            GlobalTarget = 100m,
            ActiveTables = 1,
            RuntimeMode = "Production",
            Completed = false,
            CreatedAt = start
        });
        ctx.Configurations.Add(new Configuration { Key = "MISSION_SUPPRESS_START_UNTIL_RESET", Value = "1", Pos = 991 });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var finalize = await service.FinalizeCurrentAsync("ResetDashboard");
        await service.RecordResetBoundaryAsync();
        var boundary = await service.GetResetBoundaryStateAsync();
        var session = await ctx.MissionSessions.SingleAsync();

        var pass = finalize.MissionFinalized
            && session.Completed
            && session.FinalizationReason == "ResetDashboard"
            && boundary.MissionLastResetAtUtc.HasValue
            && !boundary.MissionStartSuppressed
            && boundary.MissionSuppressStartUntilReset == "0";

        Record("4", "Reset dashboard: close + boundary + suppress cleared",
            pass,
            $"finalized={finalize.MissionFinalized}, reason={session.FinalizationReason}, lastReset={boundary.MissionLastResetAtUtc:o}, suppress={boundary.MissionSuppressStartUntilReset}, suppressed={boundary.MissionStartSuppressed}");
    }

    private async Task RunPoint5CompatibilityAsync()
    {
        await using var ctx = CreateContext();
        var service = CreateService(ctx);
        var reportBuilder = new MissionReportBuilder(ctx);

        var session = await ctx.MissionSessions.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
        if (session == null)
        {
            Record("5", "Compatibilità report", false, "Nessuna sessione disponibile per il test");
            return;
        }

        if (!await ctx.MissionMarginSamples.AnyAsync(s => s.SessionId == session.Id))
        {
            ctx.MissionMarginSamples.Add(new MissionMarginSample
            {
                SessionId = session.Id,
                Timestamp = session.StartTime,
                TotalMargin = session.TotalMargin,
                ActiveTables = session.ActiveTables,
                VmCurrent = session.TotalMargin,
                RuntimeMode = session.RuntimeMode
            });
            await ctx.SaveChangesAsync();
        }

        var current = await service.GetCurrentAsync();
        var health = await service.GetAccountingHealthAsync();
        var sessionReport = await reportBuilder.BuildSessionReportAsync(session.Id);
        var rangeReport = await reportBuilder.BuildRangeReportAsync(
            session.StartTime.Date,
            session.StartTime.Date.AddDays(2),
            session.RuntimeMode);

        var pass = current != null
            && health != null
            && sessionReport != null
            && rangeReport.Sessions.Count >= 0;

        Record("5", "MissionReportBuilder + GetCurrent + accounting-health + range report",
            pass,
            $"currentOk={current != null}, healthOpen={health.MultipleOpenSessions.Count}, sessionReportSessions={sessionReport?.Sessions.Count}, rangeSessions={rangeReport.Sessions.Count}");
    }

    private void PrintReport()
    {
        Console.WriteLine();
        Console.WriteLine("=== MISSION LIFECYCLE PREDEPLOY VALIDATION (LocalDB smoke) ===");
        foreach (var (id, label, pass, evidence) in _results)
        {
            Console.WriteLine($"[{id}] {(pass ? "PASS" : "FAIL")} — {label}");
            Console.WriteLine($"      {evidence}");
        }

        var fails = _results.Count(r => !r.Pass);
        Console.WriteLine();
        Console.WriteLine(fails == 0 ? "OVERALL: PASS" : $"OVERALL: FAIL ({fails} check(s))");
    }
}

sealed class NoopEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, IReadOnlyList<EmailAttachment>? attachments = null)
        => Task.CompletedTask;
}

sealed class NoopPushNotificationService : IPushNotificationService
{
    public PushConfigurationState GetConfigurationState() => new();
    public Task SaveSubscriptionAsync(int userId, PushSubscriptionRequest request, string? userAgent, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<int> SendMissionNotificationAsync(int sessionId, string eventType, CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task<int> SendAdminBotLevelAlertAsync(string computer, string? tavolo, int level, decimal margine, CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task<int> SendTestNotificationToUserAsync(int userId, string? deepLinkUrl, CancellationToken cancellationToken = default) => Task.FromResult(0);
}
