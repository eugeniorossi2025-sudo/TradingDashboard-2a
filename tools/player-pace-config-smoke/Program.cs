using System.Globalization;
using Decisore.Engine;
using Decisore.Services;

static void Assert(bool ok, string name)
{
    if (!ok)
    {
        Console.Error.WriteLine($"FAIL {name}");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"OK   {name}");
}

var cfgCases = new (Dictionary<string, string> cfg, double expected, string name)[]
{
    (new Dictionary<string, string>(), 107, "missing key -> 107"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "107" }, 107, "numeric string 107"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "110.5" }, 110.5, "decimal 110.5"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "" }, 107, "empty -> 107"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "abc" }, 107, "invalid text -> 107"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "0" }, 107, "zero -> 107"),
    (new Dictionary<string, string> { ["SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS"] = "-5" }, 107, "negative -> 107"),
};

foreach (var (cfg, expected, name) in cfgCases)
{
    var actual = ProactiveEngineService.ResolvePlayerP1P5ThresholdSeconds(cfg);
    Assert(Math.Abs(actual - expected) < 0.001, name);
}

var engine = new ProactiveEngine();

engine.FeedAndDecide("PC1", 1, 1, 0, 'P', 'B', 1, 1, "Sculping", 0);
var sf1 = engine.getSecurityFilterBot("PC1");
Assert(sf1?.PlayerStreakCount == 1, "P increments streak to 1");

// T does not increment
engine.FeedAndDecide("PC1", 1, 2, 0, 'T', 'B', 1, 1, "Sculping", 0);
var sfT = engine.getSecurityFilterBot("PC1");
Assert(sfT?.PlayerStreakCount == 1, "T does not increment PLAYER streak");

// P2
engine.FeedAndDecide("PC1", 1, 3, 0, 'P', 'B', 1, 1, "Sculping", 0);
Assert(engine.getSecurityFilterBot("PC1")?.PlayerStreakCount == 2, "second P increments to 2");

// B resets
engine.FeedAndDecide("PC1", 1, 4, 0, 'B', 'B', 1, 1, "Sculping", 0);
Assert(engine.getSecurityFilterBot("PC1")?.PlayerStreakCount == 0, "B resets PLAYER streak");

for (int i = 0; i < 8; i++)
    engine.FeedAndDecide("PC1", 1, 10 + i, 0, 'P', 'B', 1, 1, "Sculping", 0);
var sfLong = engine.getSecurityFilterBot("PC1");
Assert(sfLong?.PlayerStreakCount == 8, "PLAYER streak count continues beyond 5");
Assert((sfLong?.PlayerStreakIntervalSeconds?.Length ?? 0) == 4, "P1-P5 intervals computed at count >= 5");

var telemetry = engine.getTelemetry();
Assert(Math.Abs(telemetry.SecurityFilterPlayerP1P5ThresholdSeconds - 107) < 0.001, "telemetry threshold default 107");

Console.WriteLine(
    $"INFO threshold telemetry={telemetry.SecurityFilterPlayerP1P5ThresholdSeconds.ToString("0.##", CultureInfo.InvariantCulture)}s");

if (Environment.ExitCode == 0)
    Console.WriteLine("PLAYER_PACE_CONFIG_SMOKE PASS");
