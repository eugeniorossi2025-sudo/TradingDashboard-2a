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

static Advice L5Loss(ProactiveEngine engine, string pc, int hand, int martingala = 5)
    => engine.FeedAndDecide(pc, 1, hand, 0, 'P', 'B', 10, martingala, "Sculping", 0);

static Advice PbHand(ProactiveEngine engine, string pc, int hand, int martingala = 3)
    => engine.FeedAndDecide(pc, 1, hand, 0, 'B', 'B', 10, martingala, "Sculping", 0);

static Advice NewCycle(ProactiveEngine engine, string pc, int hand)
    => engine.FeedAndDecide(pc, 1, hand, 0, 'B', 'B', 1, 1, "Sculping", 0);

var engine = new ProactiveEngine
{
    SPOT_RESET_THRESHOLD_L5 = 2,
    L6_AUTH_PB_RESET_COUNTER = 600,
    SECURITY_FILTER_ENABLED = false
};

L5Loss(engine, "PC1", 1);
Assert(engine.getSecurityFilterBot("PC1")?.SpotL5PlayedCount == 1, "PC1 L5 giocata contata");
Assert(engine.getSecurityFilterBot("PC1")?.SpotL5LossCount == 1, "PC1 primo L5 => 1/2");
Assert(engine.getSecurityFilterBot("PC1")?.SpotCycleId == 1, "PC1 ciclo ID iniziale");
Assert(engine.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "PC1 1/2 => L6 NON AUTORIZZATO");
Assert(engine.getSecurityFilterBot("PC1")?.NextL5LossWillAuthorizeL6 == true, "PC1 1/2 => prossima L5 AUTORIZZA L6");

L5Loss(engine, "PC2", 10);
Assert(engine.getSecurityFilterBot("PC2")?.SpotL5LossCount == 1, "PC2 primo L5 => 1/2");
Assert(engine.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "PC1+PC2 non sommano");

var a1 = L5Loss(engine, "PC1", 2);
Assert(engine.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "PC1 2/2 => L6 AUTORIZZATO");
Assert(engine.getSecurityFilterBot("PC2")?.SpotL6Authorized == false, "PC2 resta L6 NON AUTORIZZATO");
Assert(a1.SpotL6Authorized, "PC1 advice L6 authorized");

NewCycle(engine, "PC1", 20);
Assert(engine.getSecurityFilterBot("PC1")?.SpotL5LossCount == 2, "L1 senza L6: L5 perse PC1 invariate");
Assert(engine.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "L1 senza L6: autorizzazione PC1 resta");
Assert(engine.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 0, "L1 senza L6: grant PC1 non consumato");
Assert(engine.getSecurityFilterBot("PC2")?.SpotL5LossCount == 1, "PC2 invariato dopo L1 PC1");
Assert(engine.getTelemetry().SpotL5Loss == 0, "L5 globale legacy congelato");
Assert(engine.getTelemetry().SpotPBHandsPlayed == 0, "nessun contatore PB globale SPOT");

// Ciclo SPOT PB per-bot: soglia 5 solo su PC3
var cycleOnly = new ProactiveEngine { L6_AUTH_PB_RESET_COUNTER = 5 };
for (var i = 0; i < 4; i++)
    PbHand(cycleOnly, "PC3", 200 + i);
Assert(cycleOnly.getSecurityFilterBot("PC3")?.SpotPbHandsPlayed == 4, "PC3 4 mani PB nel ciclo");
Assert(cycleOnly.getSecurityFilterBot("PC3")?.SpotCycleId == 1, "PC3 ancora ciclo 1");
PbHand(cycleOnly, "PC3", 204);
Assert(cycleOnly.getSecurityFilterBot("PC3")?.SpotPbHandsPlayed == 5, "PC3 5/5 nessun reset");
PbHand(cycleOnly, "PC3", 205);
Assert(cycleOnly.getSecurityFilterBot("PC3")?.SpotCycleId == 2, "PC3 ciclo ID++ alla mano 6 (>5)");
Assert(cycleOnly.getSecurityFilterBot("PC3")?.SpotPbHandsPlayed == 0, "PC3 PB azzerato dopo soglia");
PbHand(cycleOnly, "PC1", 1);
Assert(cycleOnly.getSecurityFilterBot("PC1")?.SpotCycleId == 1, "PC1 ciclo indipendente");
Assert(cycleOnly.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 1, "PC1 PB proprio");

// Reset ciclo PC3 non tocca L5 di PC1/PC2
var iso = new ProactiveEngine { SPOT_RESET_THRESHOLD_L5 = 2, L6_AUTH_PB_RESET_COUNTER = 5 };
L5Loss(iso, "PC1", 300);
L5Loss(iso, "PC1", 301);
L5Loss(iso, "PC2", 302);
Assert(iso.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "pre-reset PC1 L6 auth");
Assert(iso.getSecurityFilterBot("PC2")?.SpotL5LossCount == 1, "pre-reset PC2 1/2");
for (var i = 0; i < 5; i++)
    PbHand(iso, "PC3", 400 + i);
Assert(iso.getSecurityFilterBot("PC3")?.SpotPbHandsPlayed == 5, "PC3 5/5 prima reset");
PbHand(iso, "PC3", 405);
Assert(iso.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "PC1 invariato dopo reset PC3");
Assert(iso.getSecurityFilterBot("PC2")?.SpotL5LossCount == 1, "PC2 invariato dopo reset PC3");
Assert(iso.getSecurityFilterBot("PC3")?.SpotPbHandsPlayed == 0, "PC3 PB reset");
Assert(iso.getSecurityFilterBot("PC3")?.SpotL5LossCount == 0, "PC3 L5 reset");
Assert(iso.getTelemetry().SpotCyclePbHandsLimit == 5, "limite ciclo in telemetry");

// Legacy globale spento
Assert(!ProactiveEngine.LEGACY_GLOBAL_SPOT_L6_ENABLED, "LEGACY_GLOBAL_SPOT_L6_ENABLED false");
var leg = new ProactiveEngine();
L5Loss(leg, "PC1", 9001);
Assert(leg.getTelemetry().SpotL5Loss == 0, "SpotL5Loss globale congelato");

// Consumo L6: grant su 5->6, auth resta fino reset ciclo
var eGrant = new ProactiveEngine
{
    SPOT_RESET_THRESHOLD_L5 = 2,
    L6_AUTH_PB_RESET_COUNTER = 600,
    SECURITY_FILTER_ENABLED = false
};
L5Loss(eGrant, "PC1", 9002);
L5Loss(eGrant, "PC1", 9003);
Assert(eGrant.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 0, "prima L6 grant=0");
var advL6 = eGrant.FeedAndDecide("PC1", 1, 9004, 0, 'B', 'B', 10, 6, "Sculping", 0);
Assert(eGrant.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 1, "dopo passaggio L6 grant=1");
Assert(eGrant.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "auth consumata dopo passaggio L6");
Assert(eGrant.getSecurityFilterBot("PC1")?.SpotL5LossCount == 0, "L5 perse azzerate dopo consumo");
Assert(advL6.SpotL6GrantedCount == 1, "advice L6 concessi=1");
Assert(advL6.SpotL6PerBotEnabled, "advice SpotL6PerBotEnabled");
Assert(advL6.SpotCycleId >= 1, "advice SpotCycleId");
Assert(advL6.SpotL6ThresholdL5 == 2, "advice SpotL6ThresholdL5");
Assert(eGrant.getTelemetry().SpotAuthL6Counter == 0, "no credito globale operativo");

// Ciclo PB 600: 599 no reset, 600 => ciclo++
var eC = new ProactiveEngine { L6_AUTH_PB_RESET_COUNTER = 600, SPOT_RESET_THRESHOLD_L5 = 2 };
for (var i = 0; i < 598; i++)
    PbHand(eC, "PC1", 9100 + i);
Assert(eC.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 598, "PC1 598 PB");
PbHand(eC, "PC1", 9698);
Assert(eC.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 599, "PC1 599 no reset");
PbHand(eC, "PC1", 9699);
Assert(eC.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 600, "PC1 600/600 nessun reset");
PbHand(eC, "PC1", 9700);
Assert(eC.getSecurityFilterBot("PC1")?.SpotCycleId == 2, "PC1 reset alla mano 601 => ciclo 2");
Assert(eC.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 0, "PC1 PB 0 dopo reset");
L5Loss(eC, "PC2", 9700);
Assert(eC.getSecurityFilterBot("PC2")?.SpotL5LossCount == 1, "PC2 isolato da reset PC1");

// SPENTO
var off = new ProactiveEngine { SPOT_L6_PER_BOT_ENABLED = false, SPOT_RESET_THRESHOLD_L5 = 2 };
L5Loss(off, "PC1", 9800);
Assert(off.getSecurityFilterBot("PC1")?.SpotL5LossCount == 0, "SPENTO: no L5 loss");
PbHand(off, "PC1", 9801);
Assert(off.getSecurityFilterBot("PC1")?.SpotPbHandsPlayed == 0, "SPENTO: no PB spot");

// Soglia 1 e 3
var e1 = new ProactiveEngine { SPOT_RESET_THRESHOLD_L5 = 1 };
L5Loss(e1, "PC1", 9900);
Assert(e1.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "soglia 1 => 1 loss auth");
var e3 = new ProactiveEngine { SPOT_RESET_THRESHOLD_L5 = 3 };
L5Loss(e3, "PC1", 9901);
L5Loss(e3, "PC1", 9902);
Assert(e3.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "soglia 3 => 2 loss no");
L5Loss(e3, "PC1", 9903);
Assert(e3.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "soglia 3 => 3 loss auth");

// === TEST ESPLICITO CONSUMO AUTORIZZAZIONE L6 ===
Console.WriteLine();
Console.WriteLine("=== TEST CONSUMO AUTORIZZAZIONE L6 (PC1, soglia 2) ===");

static void DumpL6(string label, ProactiveEngine eng, string pc)
{
    var b = eng.getSecurityFilterBot(pc);
    Console.WriteLine($"--- {label} ---");
    Console.WriteLine($"  L5 giocate ciclo:  {b?.SpotL5PlayedCount}");
    Console.WriteLine($"  L5 perse ciclo:    {b?.SpotL5LossCount}/2");
    Console.WriteLine($"  L6 concessi ciclo: {b?.SpotL6GrantedCount}");
    Console.WriteLine($"  L6 AUTORIZZATO:    {b?.SpotL6Authorized}");
    Console.WriteLine($"  Prossima L5 persa: {b?.NextL5LossWillAuthorizeL6}");
}

var cons = new ProactiveEngine
{
    SPOT_RESET_THRESHOLD_L5 = 2,
    L6_AUTH_PB_RESET_COUNTER = 600,
    SECURITY_FILTER_ENABLED = false
};
L5Loss(cons, "PC1", 100, 5);
DumpL6("Dopo 1a L5 persa", cons, "PC1");
L5Loss(cons, "PC1", 101, 5);
DumpL6("Dopo 2a L5 persa (2/2)", cons, "PC1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "pre-L6: L6 AUTORIZZATO");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 0, "pre-L6: L6 concessi=0");

var advFirstL6 = cons.FeedAndDecide("PC1", 1, 102, 0, 'B', 'B', 10, 6, "Sculping", 0);
DumpL6("Dopo primo passaggio 5->6", cons, "PC1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 1, "post-L6: L6 concessi=1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "post-L6: auth consumata");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL5LossCount == 0, "post-L6: L5 perse 0/2");

PbHand(cons, "PC1", 103, 6);
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 1, "mano extra a L6: grant resta 1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "a L6 senza nuova auth: non autorizzato");

PbHand(cons, "PC1", 104, 5);
DumpL6("Tornato a L5 senza nuova L5 persa", cons, "PC1");
cons.FeedAndDecide("PC1", 1, 105, 0, 'B', 'B', 10, 6, "Sculping", 0);
DumpL6("Secondo passaggio 5->6 senza nuove L5 perse", cons, "PC1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 1,
    "secondo 5->6 senza 2 nuove L5: grant NON incrementato");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == false,
    "secondo 5->6 senza maturazione: L6 NON AUTORIZZATO");

L5Loss(cons, "PC1", 106, 5);
DumpL6("Dopo 1a L5 persa post-consumo", cons, "PC1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "1/2 dopo consumo: non auth");
L5Loss(cons, "PC1", 107, 5);
DumpL6("Dopo 2a L5 persa post-consumo (nuova maturazione)", cons, "PC1");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == true, "2/2 nuove L5: di nuovo AUTORIZZATO");
var advReuse = cons.FeedAndDecide("PC1", 1, 108, 0, 'B', 'B', 10, 6, "Sculping", 0);
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6GrantedCount == 2, "secondo 5->6 dopo 2 nuove L5: grant=2");
Assert(cons.getSecurityFilterBot("PC1")?.SpotL6Authorized == false, "secondo consumo: auth di nuovo spenta");

// Hot Zone + SPOT per-bot (legacy globale OFF)
static ProactiveEngine HotZoneEngine(int threshold = 2) => new()
{
    SPOT_RESET_THRESHOLD_L5 = threshold,
    HOT_ZONES = new (int from, int to)[] { (0, 20), (45, 80) },
    SECURITY_FILTER_ENABLED = false,
    PLAYER_RACE_5_FILTER_ENABLED = false,
    PLAYER_RACE_5_AC3_ENABLED = false,
    PLAYER_RACE_8_FILTER_ENABLED = false,
    PLAYER_RACE_8_AC3_ENABLED = false
};

var hzAuth = HotZoneEngine();
L5Loss(hzAuth, "HZ1", 1);
L5Loss(hzAuth, "HZ1", 2);
Assert(hzAuth.getSecurityFilterBot("HZ1")?.SpotL6Authorized == true, "hot1: SpotL6Authorized prima L5 in HZ");
var hzBlock = L5Loss(hzAuth, "HZ1", 10);
Assert(hzBlock.HotZone, "hot1: HotZone=true su mazzo 10");
Assert(hzBlock.StopL6, "hot1: L5 perso in HZ => StopL6");
Assert(hzBlock.Reason == "L6 Bloccato (Hot Zone)", "hot1: Reason Hot Zone");
Assert(hzBlock.ActionCode == 2, "hot1: ActionCode=2 (AC2)");

var hzOpen = HotZoneEngine();
L5Loss(hzOpen, "HZ2", 1);
L5Loss(hzOpen, "HZ2", 2);
var hzAllow = L5Loss(hzOpen, "HZ2", 30);
Assert(!hzAllow.HotZone, "hot2: HotZone=false su mazzo 30");
Assert(!hzAllow.StopL6, "hot2: fuori HZ => StopL6 false");
Assert(hzAllow.Reason.Contains("L6 AUTORIZZATO", StringComparison.Ordinal), "hot2: SPOT L6 autorizzato");

var hzProg = HotZoneEngine();
var hzOne = L5Loss(hzProg, "HZ3", 30);
Assert(!hzOne.StopL6, "hot3: 1/2 fuori HZ => no StopL6");
Assert(hzOne.Reason.Contains("1/2", StringComparison.Ordinal), "hot3: reason progresso SPOT");

var hzL6 = HotZoneEngine();
L5Loss(hzL6, "HZ4", 1);
L5Loss(hzL6, "HZ4", 2);
var hzSix = hzL6.FeedAndDecide("HZ4", 1, 10, 0, 'B', 'B', 10, 6, "Sculping", 0);
Assert(hzSix.HotZone, "hot4: L6 in HZ flag true");
Assert(hzSix.StopL6, "hot4: transizione 5->6 in HZ => StopL6");
Assert(hzSix.Reason == "L6 Bloccato (Hot Zone)", "hot4: reason Hot Zone su transizione");
Assert(hzL6.getSecurityFilterBot("HZ4")?.SpotL6GrantedCount == 0, "hot4: L6 bloccata => grant invariato");
Assert(hzL6.getSecurityFilterBot("HZ4")?.SpotL6Authorized == true, "hot4: autorizzazione maturata resta");
Assert(hzL6.getSecurityFilterBot("HZ4")?.SpotL5LossCount == 2, "hot4: L5 perse non consumate");

// Fase 0: consumo solo dopo gate finale
var gate = new ProactiveEngine { SPOT_RESET_THRESHOLD_L5 = 2, L6_AUTH_PB_RESET_COUNTER = 600 };
L5Loss(gate, "G1", 1);
L5Loss(gate, "G1", 2);
var gBlocked = gate.FeedAndDecide("G1", 1, 10, 0, 'B', 'B', 10, 6, "Sculping", 0);
Assert(gBlocked.StopL6, "gate HZ: StopL6 su 5->6 in HZ");
Assert(gate.getSecurityFilterBot("G1")?.SpotL6GrantedCount == 0, "gate HZ: grant non consumato");
var gOpen = new ProactiveEngine
{
    SPOT_RESET_THRESHOLD_L5 = 2,
    HOT_ZONES = new (int from, int to)[] { (0, 5) },
    SECURITY_FILTER_ENABLED = false
};
L5Loss(gOpen, "G2", 30);
L5Loss(gOpen, "G2", 31);
var gOk = gOpen.FeedAndDecide("G2", 1, 32, 0, 'B', 'B', 10, 6, "Sculping", 0);
Assert(!gOk.StopL6, "gate ok: 5->6 fuori HZ consuma");
Assert(gOpen.getSecurityFilterBot("G2")?.SpotL6GrantedCount == 1, "gate ok: grant=1");
Assert(gOpen.getSecurityFilterBot("G2")?.SpotL6Authorized == false, "gate ok: auth consumata");

if (Environment.ExitCode == 0)
    Console.WriteLine("PASS spot-reset per-bot smoke");
