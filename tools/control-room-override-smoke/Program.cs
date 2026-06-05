using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

static void Ok(string name) => Console.WriteLine($"OK   {name}");
static void Fail(string name, string? detail = null)
{
    Console.Error.WriteLine($"FAIL {name}{(detail == null ? "" : $": {detail}")}");
    Environment.ExitCode = 1;
}

static string FindWebApiDir()
{
    for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
    {
        var candidate = Path.Combine(dir.FullName, "backend", "WebApi");
        if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            return candidate;
    }
    throw new DirectoryNotFoundException("backend/WebApi not found");
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

static async Task<(int ActionCode, string CommandType)?> ConsumeAsync(SqlConnection conn, string pc)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
DELETE FROM dbo.ControlRoomCommandOverrides
OUTPUT DELETED.ActionCode, DELETED.CommandType
WHERE PC = @pc;
""";
    cmd.Parameters.AddWithValue("@pc", pc);
    await using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
        return null;
    return (reader.GetInt32(0), reader.GetString(1));
}

static async Task ClearAsync(SqlConnection conn, string pc)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = "DELETE FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc";
    cmd.Parameters.AddWithValue("@pc", pc);
    await cmd.ExecuteNonQueryAsync();
}

var webApiDir = FindWebApiDir();
var config = new ConfigurationBuilder()
    .SetBasePath(webApiDir)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.LocalProdLike.json", optional: true)
    .Build();

var cs = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(cs))
{
    Fail("connection string");
    return;
}

const string testPc = "__SMOKE_CR_OVERRIDE__";

await using var conn = new SqlConnection(cs);
try
{
    await conn.OpenAsync();
}
catch (Exception ex)
{
    Fail("SQL connect", ex.Message);
    return;
}

Ok("SQL connect");
await EnsureTableAsync(conn);
Ok("schema");

await ClearAsync(conn, testPc);

await UpsertAsync(conn, testPc, 2, "ResetMartingale");
await UpsertAsync(conn, testPc, 0, "Continue");
var first = await ConsumeAsync(conn, testPc);
if (first?.ActionCode != 0 || first.Value.CommandType != "Continue")
{
    Fail("continue consume", $"got {first?.ActionCode}/{first?.CommandType}");
    return;
}
Ok("continue AC0 one-shot");

var second = await ConsumeAsync(conn, testPc);
if (second != null)
{
    Fail("second consume should be empty");
    return;
}
Ok("consume cleared");

await UpsertAsync(conn, testPc, 0, "Continue");
await UpsertAsync(conn, testPc, 2, "ResetMartingale");
var resetAfterContinue = await ConsumeAsync(conn, testPc);
if (resetAfterContinue?.ActionCode != 2 || resetAfterContinue.Value.CommandType != "ResetMartingale")
{
    Fail("reset overwrites continue", $"got {resetAfterContinue?.ActionCode}/{resetAfterContinue?.CommandType}");
    return;
}
Ok("reset AC2 overwrites pending AC0");

await UpsertAsync(conn, testPc, 2, "ResetMartingale");
await UpsertAsync(conn, testPc, 0, "Continue");
var continueAfterReset = await ConsumeAsync(conn, testPc);
if (continueAfterReset?.ActionCode != 0 || continueAfterReset.Value.CommandType != "Continue")
{
    Fail("continue overwrites reset", $"got {continueAfterReset?.ActionCode}/{continueAfterReset?.CommandType}");
    return;
}
Ok("continue AC0 overwrites pending AC2");

await ClearAsync(conn, testPc);
Console.WriteLine("DONE control-room-override-smoke");
