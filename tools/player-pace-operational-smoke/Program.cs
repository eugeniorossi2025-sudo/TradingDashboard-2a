using Decisore.Engine;

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

static Advice FeedP(ProactiveEngine engine, string pc, int hand)
    => engine.FeedAndDecide(pc, 1, hand, 0, 'P', 'B', 1, 1, "Sculping", 0);

// SF=0, pace=1, 5x P rapidi => P1-P5 <= 107 => AC3
var paceOn = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_PACE_FILTER_ENABLED = true,
    SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS = 107
};

Advice? lastOn = null;
for (int h = 1; h <= 5; h++)
    lastOn = FeedP(paceOn, "PC1", h);

Assert(lastOn!.PlayerPaceRiskActive, "pace on + 5P => PlayerPaceRiskActive");
Assert(lastOn.PlayerPaceTriggeredAC3, "pace on + 5P <=107 => PlayerPaceTriggeredAC3");
Assert(lastOn.ActionCode == 3, "pace on + anomaly => AC3");
Assert(!lastOn.SecurityFilterActive, "SF off => SecurityFilterActive false");

// SF=0, pace=0 => nessun AC3
var paceOff = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_PACE_FILTER_ENABLED = false,
    SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS = 107
};

Advice? lastOff = null;
for (int h = 1; h <= 5; h++)
    lastOff = FeedP(paceOff, "PC2", h);

Assert(!lastOff!.PlayerPaceTriggeredAC3, "pace off + 5P => no PlayerPaceTriggeredAC3");
Assert(lastOff.ActionCode != 3, "pace off + 5P => no AC3 from player pace");

// SF=0, pace=1, solo 4P => streak incompleta => no AC3
var partial = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_PACE_FILTER_ENABLED = true,
    SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS = 107
};

Advice? lastPartial = null;
for (int h = 1; h <= 4; h++)
    lastPartial = FeedP(partial, "PC3", h);

Assert(!lastPartial!.PlayerPaceTriggeredAC3, "pace on + 4P => no AC3");
Assert(lastPartial.ActionCode != 3, "4P => ActionCode not 3");

if (Environment.ExitCode == 0)
    Console.WriteLine("PLAYER_PACE_OPERATIONAL_SMOKE PASS");
