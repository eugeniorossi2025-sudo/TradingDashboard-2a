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

static Advice L1Return(ProactiveEngine engine, string pc, int hand)
    => engine.FeedAndDecide(pc, 1, hand, 0, 'B', 'B', 10, 1, "Sculping", 0);

var e = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    L6_AUTH_PB_RESET_COUNTER = 5,
    SECURITY_FILTER_ENABLED = false
};

// 1) 2 L5 -> +1 credito
L5Loss(e, "PC1", 1);
var s1 = L5Loss(e, "PC1", 2);
var b1 = e.getSecurityFilterBot("PC1");
Assert(b1?.SpotL6CreditBalance == 1, "1) 2 L5 => +1 credito");
Assert(s1.ActionCode != 9, "10) no decide=9 (scenario 1)");

// 2) 4 L5 -> +2 crediti
L5Loss(e, "PC1", 3);
L5Loss(e, "PC1", 4);
b1 = e.getSecurityFilterBot("PC1");
Assert(b1?.SpotL6CreditBalance == 2, "2) 4 L5 => +2 crediti");

// 3) 5->6 consuma 1 credito
var aL6 = e.FeedAndDecide("PC1", 1, 30, 0, 'B', 'B', 10, 6, "Sculping", 0);
b1 = e.getSecurityFilterBot("PC1");
Assert(aL6.StopL6 == false && aL6.ActionCode != 2, "3) 5->6 concessa");
Assert(b1?.SpotL6CreditBalance == 1, "3) 5->6 consuma 1 credito");
Assert(b1?.SpotL6GrantedCount == 1, "3) grant incrementato");

// 4) 2 crediti + 1 L6 -> resta 1 credito (gia' verificato sopra)
Assert(b1?.SpotL6CreditBalance == 1, "4) da 2 crediti dopo 1 L6 resta 1");

// 5) L6 bloccata da HZ -> credito invariato (consumo resta post-gate)
var hz = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    SECURITY_FILTER_ENABLED = false,
    HOT_ZONES = new (int from, int to)[] { (0, 20) }
};
L5Loss(hz, "HZ1", 1);
L5Loss(hz, "HZ1", 2); // +1 credito
var hzPre = hz.getSecurityFilterBot("HZ1");
var hzL6 = hz.FeedAndDecide("HZ1", 1, 10, 0, 'B', 'B', 10, 6, "Sculping", 0);
var hzPost = hz.getSecurityFilterBot("HZ1");
Assert(hzL6.StopL6 && hzL6.ActionCode == 2, "5) HZ blocca L6");
Assert(hzPre?.SpotL6CreditBalance == hzPost?.SpotL6CreditBalance, "5) HZ: credito invariato");

// 5b) L6 bloccata da SF -> credito invariato
var sf = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    SECURITY_FILTER_ENABLED = true,
    SECURITY_FILTER_MIN_SCORE = 1,
    SECURITY_FILTER_MIN_STREAK = 1,
    SECURITY_FILTER_MAX_SHOE_HAND = 9999,
    SECURITY_FILTER_MAX_AVG_SECONDS = 9999,
    SECURITY_FILTER_VERY_FAST_SECONDS = 9999
};
L5Loss(sf, "SF1", 1);
L5Loss(sf, "SF1", 2); // +1 credito
var sfPre = sf.getSecurityFilterBot("SF1");
var sfL6 = sf.FeedAndDecide("SF1", 1, 3, 0, 'B', 'B', 10, 6, "Sculping", 0);
var sfPost = sf.getSecurityFilterBot("SF1");
Assert(sfL6.ActionCode == 3 || sfL6.StopL6, "5b) SF blocca L6");
Assert(sfPre?.SpotL6CreditBalance == sfPost?.SpotL6CreditBalance, "5b) SF: credito invariato");

// 6) ritorno L1 senza L6 -> credito invariato
var l1 = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    SECURITY_FILTER_ENABLED = false
};
L5Loss(l1, "L1A", 1);
L5Loss(l1, "L1A", 2); // +1 credito
var l1Pre = l1.getSecurityFilterBot("L1A");
var l1Adv = L1Return(l1, "L1A", 3);
var l1Post = l1.getSecurityFilterBot("L1A");
Assert(l1Adv.ActionCode != 9, "10) no decide=9 (L1)");
Assert(l1Pre?.SpotL6CreditBalance == l1Post?.SpotL6CreditBalance, "6) L1 senza L6 conserva credito");

// 7) fine ciclo SPOT reset solo bot corrente (credito/l5/grant azzerati)
var cyc = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    L6_AUTH_PB_RESET_COUNTER = 3,
    SECURITY_FILTER_ENABLED = false
};
L5Loss(cyc, "A", 100);
L5Loss(cyc, "A", 101); // credito A=1
cyc.FeedAndDecide("A", 1, 102, 0, 'B', 'B', 10, 6, "Sculping", 0); // grant A=1, credito=0
L5Loss(cyc, "A", 103);
L5Loss(cyc, "A", 104); // credito A=1
L5Loss(cyc, "B", 200); // stato B separato
var preA = cyc.getSecurityFilterBot("A");
var preB = cyc.getSecurityFilterBot("B");
PbHand(cyc, "A", 105);
PbHand(cyc, "A", 106);
PbHand(cyc, "A", 107);
PbHand(cyc, "A", 108); // mano > limite => reset ciclo A
var postA = cyc.getSecurityFilterBot("A");
var postB = cyc.getSecurityFilterBot("B");
Assert(preA?.SpotCycleId < postA?.SpotCycleId, "7) ciclo A incrementato");
Assert(postA?.SpotL6CreditBalance == 0 && postA?.SpotL5LossCount == 0 && postA?.SpotL6GrantedCount == 0, "7) reset A: credito/L5/grant azzerati");
Assert(preB?.SpotL5LossCount == postB?.SpotL5LossCount, "7) reset A non tocca B");

// 8) PC isolati
var iso = new ProactiveEngine
{
    SPOT_L6_CREDIT_L5_REQUIRED = 2,
    SPOT_L6_CREDITS_GENERATED = 1,
    SECURITY_FILTER_ENABLED = false
};
L5Loss(iso, "I1", 1);
L5Loss(iso, "I1", 2); // credito I1=1
L5Loss(iso, "I2", 3); // I2 solo 1 loss
Assert(iso.getSecurityFilterBot("I1")?.SpotL6CreditBalance == 1, "8) I1 ha 1 credito");
Assert(iso.getSecurityFilterBot("I2")?.SpotL6CreditBalance == 0, "8) I2 non ereditata");

// 9) legacy globale resta OFF
Assert(!ProactiveEngine.LEGACY_GLOBAL_SPOT_L6_ENABLED, "9) legacy globale OFF");
Assert(iso.getTelemetry().SpotL5Loss == 0, "9) telemetria legacy congelata");

// 10) nessun decide=9 in baseline
var baseAdv = e.FeedAndDecide("BASE", 1, 999, 0, 'B', 'B', 10, 1, "ATTESA", 0);
Assert(baseAdv.ActionCode != 9, "10) nessun decide=9 baseline");

if (Environment.ExitCode == 0)
    Console.WriteLine("PASS spot-reset per-bot smoke");
