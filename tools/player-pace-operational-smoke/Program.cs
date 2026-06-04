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

static Advice Feed(char esito, ProactiveEngine engine, string pc, int hand)
    => engine.FeedAndDecide(pc, 1, hand, 0, esito, 'B', 1, 1, "Sculping", 0);

// Filtro 5 ON + AC3 5 OFF => avviso P5, no AC3
var p5AlertOnly = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = true,
    PLAYER_RACE_5_AC3_ENABLED = false,
    PLAYER_RACE_8_FILTER_ENABLED = false,
    PLAYER_RACE_8_AC3_ENABLED = false
};
Advice? at5 = null;
for (int h = 1; h <= 5; h++)
    at5 = Feed('P', p5AlertOnly, "PC1", h);
Assert(p5AlertOnly.getSecurityFilterBot("PC1")?.PlayerRace5Alert == true, "F5 ON AC35 OFF => alert P5");
Assert(at5!.PlayerRace5Triggered, "F5 ON => advice alert");
Assert(!at5.PlayerRace5Ac3Triggered, "AC35 OFF => no AC3 at P5");
Assert(at5.ActionCode != 3, "AC35 OFF => no ActionCode 3 at P5");

// Filtro 5 ON + AC3 5 ON => avviso + AC3 a P5
var p5Both = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = true,
    PLAYER_RACE_5_AC3_ENABLED = true,
    PLAYER_RACE_8_FILTER_ENABLED = false,
    PLAYER_RACE_8_AC3_ENABLED = false
};
Advice? at5Ac3 = null;
for (int h = 1; h <= 5; h++)
    at5Ac3 = Feed('P', p5Both, "PC2", h);
Assert(at5Ac3!.PlayerRace5Ac3Triggered, "F5+AC35 ON => AC3 at P5");
Assert(at5Ac3.ActionCode == 3, "AC35 ON => ActionCode 3 at P5");

// Filtro 8 ON + AC3 8 OFF => avviso P8, no AC3
var p8AlertOnly = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = false,
    PLAYER_RACE_5_AC3_ENABLED = false,
    PLAYER_RACE_8_FILTER_ENABLED = true,
    PLAYER_RACE_8_AC3_ENABLED = false
};
Advice? at8 = null;
for (int h = 1; h <= 8; h++)
    at8 = Feed('P', p8AlertOnly, "PC3", h);
Assert(p8AlertOnly.getSecurityFilterBot("PC3")?.PlayerRace8Alert == true, "F8 ON AC38 OFF => alert P8");
Assert(!at8!.PlayerRace8Ac3Triggered, "AC38 OFF => no AC3 at P8");
Assert(at8.ActionCode != 3, "AC38 OFF => no ActionCode 3 at P8");

// Filtro 8 ON + AC3 8 ON => avviso + AC3 a P8
var p8Both = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = false,
    PLAYER_RACE_5_AC3_ENABLED = false,
    PLAYER_RACE_8_FILTER_ENABLED = true,
    PLAYER_RACE_8_AC3_ENABLED = true
};
Advice? at8Ac3 = null;
for (int h = 1; h <= 8; h++)
    at8Ac3 = Feed('P', p8Both, "PC4", h);
Assert(at8Ac3!.PlayerRace8Ac3Triggered, "F8+AC38 ON => AC3 at P8");
Assert(at8Ac3.ActionCode == 3, "AC38 ON => ActionCode 3 at P8");

// Filtro 5 OFF => nessun avviso P5 (AC3 5 resta indipendente)
var p5Off = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = false,
    PLAYER_RACE_5_AC3_ENABLED = true
};
Advice? p5OffAdvice = null;
for (int h = 1; h <= 5; h++)
    p5OffAdvice = Feed('P', p5Off, "PC5", h);
Assert(!p5OffAdvice!.PlayerRace5Triggered, "F5 OFF => no alert P5");
Assert(p5OffAdvice.PlayerRace5Ac3Triggered, "AC35 ON alone => AC3 at P5 without alert");

// Filtro 8 OFF => nessun avviso P8
var p8Off = new ProactiveEngine
{
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_8_FILTER_ENABLED = false,
    PLAYER_RACE_8_AC3_ENABLED = false
};
Advice? p8OffAdvice = null;
for (int h = 1; h <= 8; h++)
    p8OffAdvice = Feed('P', p8Off, "PC6", h);
Assert(!p8OffAdvice!.PlayerRace8Alert, "F8 OFF => no alert P8");
Assert(!p8OffAdvice.PlayerRace8Ac3Triggered, "F8+AC38 OFF => no AC3 at P8");

// B reset, T neutral
var reset = new ProactiveEngine
{
    PLAYER_RACE_5_FILTER_ENABLED = true,
    PLAYER_RACE_8_FILTER_ENABLED = true
};
for (int h = 1; h <= 4; h++)
    Feed('P', reset, "PC7", h);
Feed('B', reset, "PC7", 5);
Assert(reset.getSecurityFilterBot("PC7")?.PlayerStreakCount == 0, "B resets");

var tie = new ProactiveEngine { PLAYER_RACE_5_FILTER_ENABLED = true };
for (int h = 1; h <= 4; h++)
    Feed('P', tie, "PC8", h);
Feed('T', tie, "PC8", 5);
Assert(tie.getSecurityFilterBot("PC8")?.PlayerStreakCount == 4, "T neutral keeps streak");

if (Environment.ExitCode == 0)
    Console.WriteLine("PASS player-race 4-flag operational smoke");
