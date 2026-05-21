using System;
using System.Collections.Generic;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1
{
    public enum Signal
    {
        Green,
        YellowOrRed
    }

    // ============================================================
    // 🔺 SETTINGS GLOBALI (completo)
    // ============================================================
    public sealed class ProactiveSettings
    {
        // Montante (invariato)
        public int[] Levels { get; set; } = new[] { 1, 3, 7, 15, 35, 75, 155, 340 };
        public double K { get; set; } = 1.0;

        // Finestra run
        public int WindowW10 { get; set; } = 20;
        public int MaxRunPAllowed { get; set; } = 2;
        public int MaxRunSideAllowedTable { get; set; } = 3;

        // Hot zones (invariato)
        public (int start, int end)[] HotZones { get; set; } =
            new (int, int)[] { (11, 20), (41, 50), (51, 60), (61, 70) };

        // 🔥 Soglie stabilizzate
        public int HighThresh { get; set; } = 250;
        public int LowThresh { get; set; } = -300;

        // 🔥 HEAVY CONTROL — Anti-cluster (Hmax=1)
        public int HmaxHigh { get; set; } = 1;
        public int HmaxMid { get; set; } = 1;
        public int HmaxLow { get; set; } = 1;

        public int CooldownHigh { get; set; } = 4;
        public int CooldownMid { get; set; } = 3;
        public int CooldownLow { get; set; } = 2;

        // Costi di L5 (invariato)
        public int L5LossUnits { get; set; } = 61;

        // 🔥 Override — 1 tentativo max / shoe
        public int MaxHotOverridesConcurrent { get; set; } = 0;
        public int MaxHotOverridesPerShoe { get; set; } = 1;
        public double DebtTriggerRatio { get; set; } = 0.60;

        // 🔥 VM reale
        public double MeanUnitsPerHandPerTable { get; set; } = 0.50;
        public int EstimatedHandsLeftPerTable { get; set; } = 35;

        // 🔥 SCUDERIA EXTENSIONS — stabilità
        public int SyncDelayMs { get; set; } = 120;
        public int HeavyDecayAfterHands { get; set; } = 5;
        public bool ResetOnMapChange { get; set; } = true;
        public int GlobalHeavyCapWindow { get; set; } = 60;
        public int GlobalHeavyCap { get; set; } = 4;
        public int PerTableHeavyLimit { get; set; } = 2;
    }
}

// ============================================================
// 🔺 OUTPUT ADVICE (con Prediction + TableStatus)
// ============================================================
[Serializable]
public sealed class Advice
{
    public int TableId { get; set; }
    public int LevelIndex { get; set; }
    public double StakeUnits { get; set; }
    public double GlobalMargin { get; set; }

    public bool StopAtL5 { get; set; }
    public bool AuthorizedHeavy { get; set; }
    public string Reason { get; set; } = "";
    public string SignalW10 { get; set; } = "Green";
    public string SignalTableW10 { get; set; } = "Green";
    public bool HotZone { get; set; } = false;
    public string TooltipJson { get; set; } = "";
    public string HotZoneLabel { get; set; } = "";

    public int PortfolioDebtUnits { get; set; } = 0;
    public int HotOverridesActive { get; set; } = 0;
    public int HotOverridesUsedThisShoe { get; set; } = 0;

    public double VmLocal20 { get; set; } = 0.0;
    public string Prediction { get; set; } = "";

    // ===== [SCUDERIA ADD] Stato visivo per dashboard =====
    public string TableStatus { get; set; } = "🟢 Active";
    // ======================================================
}

// ============================================================
// 🔺 ROWSTATE – stato completo per tavolo
// ============================================================
[Serializable]
public sealed class RowState
{
    public int? PrevMazzo { get; set; }
    public int PrevLevel { get; set; }
    public decimal PrevMargine { get; set; }
    public int PrevStake { get; set; }
    public string PrevSignalW10 { get; set; }
    public bool PrevHotZone { get; set; }
    public Queue<char> History { get; set; } = new();
    public Queue<char> HistoryTable { get; set; } = new();
    public int RunP { get; set; } = 0;

    public bool ForceToL8Active { get; set; } = false;
    public int L5ClosedCount { get; set; } = 0;

    // ===== [SCUDERIA ADD] Vm locale (20 mani)
    public int HandCount { get; set; } = 0;
    public double MargineAccum { get; set; } = 0.0;
    public double VmLocal20 { get; set; } = 0.0;

    // ===== [SCUDERIA ADD] Validazione e riattivazione tavoli =====
    public int WarmInputs { get; set; } = 0; // conta i primi input di warm-up
    public int InvalidCount { get; set; } = 0; // errori consecutivi
    public int ValidRecovery { get; set; } = 0; // conteggio buoni

    public bool Disabled { get; set; } = false; // se tavolo in pausa
    // ==============================================================
}

// ============================================================
// 🔺 OUTCOME INFERER – invariato ma con tolleranza 0.6
// ============================================================
public static class OutcomeInferer
{
    private static bool Approx(decimal x, decimal y, decimal tol = 0.6m)
        => Math.Abs(x - y) <= tol;

    public static int ToLevelIndex(int martingalaUi)
    {
        if (martingalaUi >= 1 && martingalaUi <= 8)
            return martingalaUi - 1;
        int max = 7, min = 0;
        return Math.Max(Math.Min(martingalaUi, max), min);
    }

    public static char InferOutcome(RowState s, int levelIdxNow, decimal margineNow)
    {
        if (s.PrevMazzo is null) return 'T';
        var dM = margineNow - s.PrevMargine;
        var dL = levelIdxNow - s.PrevLevel;

        if (Approx(dM, 0m) && dL == 0) return 'T';
        if ((levelIdxNow == 0 || dL < 0) && Approx(dM, +s.PrevStake)) return 'B';
        if (dL >= 1 && Approx(dM, -s.PrevStake)) return 'P';
        if (dL > 0) return 'P';
        if (dL < 0 || levelIdxNow == 0) return 'B';
        return 'T';
    }
}