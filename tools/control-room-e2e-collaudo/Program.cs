using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

static void Ok(string name, string? detail = null) =>
    Console.WriteLine($"OK   {name}{(detail == null ? "" : $" — {detail}")}");
static void Fail(string name, string? detail = null)
{
    Console.Error.WriteLine($"FAIL {name}{(detail == null ? "" : $" — {detail}")}");
    Environment.ExitCode = 1;
}
static void Log(string line) => Console.WriteLine($"LOG  {line}");
static bool IsEngineErrorAction(int? action) => action == 9;

static string FindDecisoreDir()
{
    for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
    {
        var candidate = Path.Combine(dir.FullName, "decision-engine", "Decisore");
        if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            return candidate;
    }
    throw new DirectoryNotFoundException("decision-engine/Decisore not found");
}

static async Task EnsureTableAsync(SqlConnection conn)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
IF OBJECT_ID(N'[dbo].[ControlRoomCommandOverrides]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ControlRoomCommandOverrides](
        [PC] NVARCHAR(50) NOT NULL CONSTRAINT [PK_ControlRoomCommandOverrides] PRIMARY KEY,
        [ActionCode] INT NOT NULL,
        [CommandType] NVARCHAR(32) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_ControlRoomCommandOverrides_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [CreatedByUserId] INT NULL
    );
END;
""";
    await cmd.ExecuteNonQueryAsync();
}

static async Task UpsertAsync(SqlConnection conn, string pc, int actionCode, string commandType)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
IF EXISTS (SELECT 1 FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc)
    UPDATE dbo.ControlRoomCommandOverrides
    SET ActionCode = @ac, CommandType = @ct, CreatedAtUtc = SYSUTCDATETIME()
    WHERE PC = @pc;
ELSE
    INSERT INTO dbo.ControlRoomCommandOverrides (PC, ActionCode, CommandType, CreatedAtUtc)
    VALUES (@pc, @ac, @ct, SYSUTCDATETIME());
""";
    cmd.Parameters.AddWithValue("@pc", pc);
    cmd.Parameters.AddWithValue("@ac", actionCode);
    cmd.Parameters.AddWithValue("@ct", commandType);
    await cmd.ExecuteNonQueryAsync();
}

static async Task<int> CountPendingAsync(SqlConnection conn, string pc)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT COUNT(1) FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc";
    cmd.Parameters.AddWithValue("@pc", pc);
    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
}

static async Task ClearPcAsync(SqlConnection conn, string pc)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = "DELETE FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc";
    cmd.Parameters.AddWithValue("@pc", pc);
    await cmd.ExecuteNonQueryAsync();
}

static Dictionary<string, string> BuildDecideParams(string username, string password, string computer)
{
    return new Dictionary<string, string>
    {
        ["USERNAME"] = username,
        ["PASSWORD"] = password,
        ["COMPUTER"] = computer,
        ["TAVOLO"] = "1",
        ["SALDO_INIZIALE"] = "1000",
        ["SALDO_ISTANTANEO"] = "1000",
        ["MARGINE"] = "0",
        ["STATO"] = "ATTESA",
        ["COLPO_MARTINGALA"] = "0",
        ["MAZZO"] = "5",
        ["TEMPO"] = "00:02",
        ["VALORE_GIOCATO"] = "10",
        ["PBT"] = "P",
        ["CHOSEN_COLOR"] = ""
    };
}

static async Task<int?> CallDecideAsync(HttpClient http, string baseUrl, Dictionary<string, string> qp)
{
    var qs = string.Join("&", qp.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
    var res = await http.GetAsync($"{baseUrl.TrimEnd('/')}/api/proactive/decide?{qs}");
    var text = (await res.Content.ReadAsStringAsync()).Trim();
    if (!res.IsSuccessStatusCode)
        throw new InvalidOperationException($"decide HTTP {(int)res.StatusCode}: {text}");
    return int.TryParse(text, out var n) ? n : null;
}

static async Task<string?> ReadLatestApiLogAsync(SqlConnection conn, string category, string contains)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
SELECT TOP 1 Description
FROM dbo.ApiLogs
WHERE Category = @cat AND Description LIKE @like
ORDER BY CreatedAt DESC;
""";
    cmd.Parameters.AddWithValue("@cat", category);
    cmd.Parameters.AddWithValue("@like", $"%{contains}%");
    var o = await cmd.ExecuteScalarAsync();
    return o as string;
}

static async Task<bool> ProbeDecideAsync(HttpClient http, string baseUrl, Dictionary<string, string> qp)
{
    try
    {
        var cmd = await CallDecideAsync(http, baseUrl, qp);
        return cmd != null;
    }
    catch
    {
        return false;
    }
}

var decisoreDir = FindDecisoreDir();
var config = new ConfigurationBuilder()
    .SetBasePath(decisoreDir)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var cs = Environment.GetEnvironmentVariable("COLLAUDO_CONNECTION_STRING")
    ?? config.GetConnectionString("DefaultConnection");
var baseUrl = Environment.GetEnvironmentVariable("DECISORE_URL") ?? "http://127.0.0.1";
var username = Environment.GetEnvironmentVariable("DECIDE_USERNAME") ?? "eugenio";
var password = Environment.GetEnvironmentVariable("DECIDE_PASSWORD") ?? "123456";

if (string.IsNullOrWhiteSpace(cs))
{
    Fail("connection string");
    return;
}

const string pc1 = "CR_E2E_PC1";
const string pc2 = "CR_E2E_PC2";
const string pc3 = "CR_E2E_PC3";
const string pc4 = "CR_E2E_PC4";
var testPcs = new[] { pc1, pc2, pc3, pc4 };

await using var conn = new SqlConnection(cs);
await conn.OpenAsync();
Ok("SQL connect");
await EnsureTableAsync(conn);

foreach (var pc in testPcs)
    await ClearPcAsync(conn, pc);

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
var qp2 = BuildDecideParams(username, password, pc2);
if (!await ProbeDecideAsync(http, baseUrl, qp2))
{
    Fail("decisore reachable", $"GET {baseUrl}/api/proactive/decide — avvia Decisore locale patchato o imposta DECISORE_URL");
    return;
}
Ok("decisore reachable", baseUrl);

// Baseline senza override
var baseline1 = await CallDecideAsync(http, baseUrl, qp2);
if (baseline1 == null)
{
    Fail("baseline decide parse");
    return;
}
Ok("baseline decide PC2", $"engineAction={baseline1}");
if (IsEngineErrorAction(baseline1))
{
    Fail("decide engine/config", "response=9 — patch Decisore non deployata o DB/config incompleto (non usare LocalDB)");
    return;
}
Log($"DECIDE baseline pc={pc2} response={baseline1} (no override)");

var pendingBefore = await CountPendingAsync(conn, pc2);
if (pendingBefore != 0)
    Fail("PC2 pending clean before test", $"count={pendingBefore}");

// --- CONTINUA (AC0) one-shot ---
Log($"UI_SIM click CONTINUA pc={pc2}");
await UpsertAsync(conn, pc2, 0, "Continue");
Log($"BACKEND_SIM SetContinue pc={pc2} actionCode=0 commandType=Continue");
Ok("backend continue queued", $"pending={await CountPendingAsync(conn, pc2)}");

var decideContinue = await CallDecideAsync(http, baseUrl, qp2);
if (decideContinue != 0)
{
    Fail("CONTINUA first decide", $"expected 0 got {decideContinue}");
    return;
}
Ok("CONTINUA first decide", "overrideAction=0");

var apiLogContinue = await ReadLatestApiLogAsync(conn, pc2, "CONTROL_ROOM_OVERRIDE");
if (string.IsNullOrWhiteSpace(apiLogContinue) ||
    !apiLogContinue.Contains("engineAction=", StringComparison.Ordinal) ||
    !apiLogContinue.Contains("overrideAction=0", StringComparison.Ordinal) ||
    !apiLogContinue.Contains("commandType=Continue", StringComparison.Ordinal) ||
    !apiLogContinue.Contains("consumed=true", StringComparison.Ordinal))
{
    Fail("CONTINUA decide log", apiLogContinue?[..Math.Min(200, apiLogContinue?.Length ?? 0)]);
    return;
}
Ok("CONTINUA decide log", apiLogContinue.Split('\n').FirstOrDefault(l => l.Contains("CONTROL_ROOM_OVERRIDE"))?.Trim());

if (await CountPendingAsync(conn, pc2) != 0)
    Fail("CONTINUA DB after consume", "override still pending");

var decideAfterContinue = await CallDecideAsync(http, baseUrl, qp2);
Ok("CONTINUA second decide", $"response={decideAfterContinue} (engine normal, no override)");
Log($"DECIDE post-consume pc={pc2} response={decideAfterContinue} pending={await CountPendingAsync(conn, pc2)}");

// --- Isolamento: AC0 su PC2 non consumato da decide PC1/PC3 ---
await UpsertAsync(conn, pc2, 0, "Continue");
var qp1 = BuildDecideParams(username, password, pc1);
var qp3 = BuildDecideParams(username, password, pc3);
var other1 = await CallDecideAsync(http, baseUrl, qp1);
var other3 = await CallDecideAsync(http, baseUrl, qp3);
if (await CountPendingAsync(conn, pc2) != 1)
    Fail("isolation PC2 pending after PC1/PC3 decide");
Ok("isolation AC0 PC2", $"PC1 decide={other1} PC3 decide={other3} PC2 still pending=1");

// --- AZZERA (AC2) one-shot ---
await ClearPcAsync(conn, pc2);
Log($"UI_SIM click AZZERA MARTINGALA pc={pc2}");
await UpsertAsync(conn, pc2, 2, "ResetMartingale");
Log($"BACKEND_SIM SetResetMartingale pc={pc2} actionCode=2 commandType=ResetMartingale");

var decideReset = await CallDecideAsync(http, baseUrl, qp2);
if (decideReset != 2)
{
    Fail("AZZERA first decide", $"expected 2 got {decideReset}");
    return;
}
Ok("AZZERA first decide", "overrideAction=2");

var apiLogReset = await ReadLatestApiLogAsync(conn, pc2, "CONTROL_ROOM_OVERRIDE");
if (string.IsNullOrWhiteSpace(apiLogReset) ||
    !apiLogReset.Contains("overrideAction=2", StringComparison.Ordinal) ||
    !apiLogReset.Contains("commandType=ResetMartingale", StringComparison.Ordinal))
{
    Fail("AZZERA decide log", apiLogReset?[..Math.Min(200, apiLogReset?.Length ?? 0)]);
    return;
}
Ok("AZZERA decide log", apiLogReset.Split('\n').FirstOrDefault(l => l.Contains("CONTROL_ROOM_OVERRIDE"))?.Trim());

if (await CountPendingAsync(conn, pc2) != 0)
    Fail("AZZERA DB after consume");

var decideAfterReset = await CallDecideAsync(http, baseUrl, qp2);
Ok("AZZERA second decide", $"response={decideAfterReset} (engine normal)");

// --- Isolamento AC2 su PC2: PC4 decide non consuma ---
await UpsertAsync(conn, pc2, 2, "ResetMartingale");
var qp4 = BuildDecideParams(username, password, pc4);
var other4 = await CallDecideAsync(http, baseUrl, qp4);
if (await CountPendingAsync(conn, pc2) != 1)
    Fail("isolation AC2 PC2 after PC4 decide");
Ok("isolation AC2 PC2", $"PC4 decide={other4} PC2 still pending=1");

// --- Overwrite: AC2 poi CONTINUA => prossimo decide 0 ---
await UpsertAsync(conn, pc2, 2, "ResetMartingale");
await UpsertAsync(conn, pc2, 0, "Continue");
var overwrite = await CallDecideAsync(http, baseUrl, qp2);
if (overwrite != 0)
    Fail("overwrite continue over reset", $"got {overwrite}");
Ok("overwrite AC0 over AC2", "decide=0");

// --- Overwrite: AC0 poi AZZERA => prossimo decide 2 ---
await UpsertAsync(conn, pc2, 0, "Continue");
await UpsertAsync(conn, pc2, 2, "ResetMartingale");
var overwrite2 = await CallDecideAsync(http, baseUrl, qp2);
if (overwrite2 != 2)
    Fail("overwrite reset over continue", $"got {overwrite2}");
Ok("overwrite AC2 over AC0", "decide=2");

// --- No override: decide identico baseline pattern ---
await ClearPcAsync(conn, pc2);
var noOverrideA = await CallDecideAsync(http, baseUrl, qp2);
var noOverrideB = await CallDecideAsync(http, baseUrl, qp2);
if (noOverrideA != noOverrideB)
    Fail("no-override stability", $"{noOverrideA} vs {noOverrideB}");
Ok("no-override identical consecutive", $"action={noOverrideA}");

foreach (var pc in testPcs)
    await ClearPcAsync(conn, pc);

Console.WriteLine("VERDICT PASS — control-room override manual, one-shot, per-PC, consumed");
