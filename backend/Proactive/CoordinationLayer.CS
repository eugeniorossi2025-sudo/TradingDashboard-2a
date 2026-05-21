using System;
using System.Collections.Generic;
using System.Linq;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1;

public sealed class CoordinationLayer
{
    private int _recentValveTicks = 0;

    // Classe di telemetria utile per debug/dashboard
    public sealed class CoordinationInfo
    {
        public int TablesStressed { get; set; }
        public int SevereTables { get; set; }
        public int SimulHeavyCandidates { get; set; }
        public double Vm { get; set; } // in UNITÀ/min globale
        public double VmTarget { get; set; } // in UNITÀ/min target globale
        public bool VmValveTriggered { get; set; }

        public double VmLocal20Avg { get; set; }
        public double VmLocal20PositiveRate { get; set; }

        // NEW – SIGNALS FOR DASHBOARD
        public Dictionary<int, string> SignalW10Map { get; set; }
        public Dictionary<int, bool> HotZoneMap { get; set; }
        public Dictionary<int, int> TableScoreMap { get; set; }
    }

    // ============================================================
    // 🔺 APPLY COORDINATED ADJUSTMENTS (migliorato)
    // ============================================================
    public CoordinationInfo ApplyCoordinatedAdjustments(
        ProactiveSettings s,
        IDictionary<int, RowState> rows,
        int totalTables,
        double elapsedMinutes,
        double globalMarginUnits) // 🔸 margine in UNITÀ
    {
        var vm = globalMarginUnits / Math.Max(1.0, elapsedMinutes);
        var vmTarget = 50.0 * Math.Max(1, totalTables) / 180.0; // 50 u/tavolo in 180'

        var stressed = rows.Values.Count(r => r.RunP >= 3);
        var severe = rows.Values.Count(r => IsSevereRedTable(r, s));
        var heavyCandidates = rows.Values.Count(r => r.RunP == 0 && r.History.Count >= s.WindowW10);

        // Cross-damping: se troppi tavoli stressati, frena leggermente
        if (stressed >= 3)
        {
            s.HmaxLow = Math.Max(1, s.HmaxLow - 1);
            s.HmaxMid = Math.Max(1, s.HmaxMid - 1);
            s.CooldownMid = Math.Min(s.CooldownMid + 1, s.CooldownHigh + 2);
        }

        // Congestione preventiva
        if (severe >= 2 && heavyCandidates >= 3)
        {
            s.CooldownMid = Math.Min(s.CooldownMid + 1, s.CooldownHigh + 2);
            s.DebtTriggerRatio = Math.Min(0.75, s.DebtTriggerRatio + 0.05);
        }

        // ============================================================
        // 🔺 Raffreddamento sincronizzato multi-tavolo
        // ============================================================
        if (severe >= 2)
            s.CooldownMid = Math.Max(s.CooldownMid, 2);

        // Valvola Vm (anti-spirale)
        var valve = TryVmCompensationValve(s, rows, totalTables, vm, vmTarget);
        if (valve)
            _recentValveTicks = 3;
        else
            _recentValveTicks = Math.Max(0, _recentValveTicks - 1);

        // ============================================================
        // 🔺 [SCUDERIA ADD] Telemetria Vm locale per dashboard
        // ============================================================
        var vmLocal20Avg = 0.0;
        var vmLocal20PositiveRate = 0.0;
        var vmVals = rows.Values.Select(r => r.VmLocal20).Where(v => !double.IsNaN(v)).ToList();
        if (vmVals.Count > 0)
        {
            vmLocal20Avg = vmVals.Average();
            vmLocal20PositiveRate = (double)vmVals.Count(v => v > 0) / vmVals.Count;
        }

        // NEW SIGNAL MAPS FOR DASHBOARD
        var signalMap = rows.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.PrevSignalW10 // must be added in RowState
        );

        var hotMap = rows.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.PrevHotZone
        );

        var scoreMap = rows.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.HistoryTable?.Count ?? 0
        );

        return new CoordinationInfo
        {
            TablesStressed = stressed,
            SevereTables = severe,
            SimulHeavyCandidates = heavyCandidates,
            Vm = vm,
            VmTarget = vmTarget,
            VmValveTriggered = valve,
            VmLocal20Avg = Math.Round(vmLocal20Avg, 3),
            VmLocal20PositiveRate = Math.Round(vmLocal20PositiveRate * 100.0, 1),
            SignalW10Map = signalMap,
            HotZoneMap = hotMap,
            TableScoreMap = scoreMap
        };
    }

    // ============================================================
    // Valvola compensativa Vm (anti-spirale)
    // ============================================================
    private bool TryVmCompensationValve(ProactiveSettings s, IDictionary<int, RowState> rows,
        int totalTables, double vm, double vmTarget)
    {
        if (_recentValveTicks > 0) return false;

        var vmTooLow = vm < vmTarget * 0.80;
        var systemTooCold = (s.HmaxLow <= 1 && s.HmaxMid <= 1) || s.CooldownMid >= 2;
        if (!(vmTooLow && systemTooCold)) return false;

        var anyColdTable = rows.Values.Any(r => r.RunP == 0 && r.History.Count >= s.WindowW10);
        if (!anyColdTable) return false;

        s.HmaxMid = Math.Max(s.HmaxMid, 2);
        s.CooldownMid = Math.Max(1, s.CooldownMid - 1);
        s.DebtTriggerRatio = Math.Max(0.50, s.DebtTriggerRatio - 0.05);
        return true;
    }

    // ============================================================
    // Analisi tavoli "rossi severi"
    // ============================================================
    private bool IsSevereRedTable(RowState rs, ProactiveSettings s)
    {
        if (rs.HistoryTable == null || rs.HistoryTable.Count == 0) return false;
        int cur = 0, maxRun = 0;
        var last = '\0';
        foreach (var o in rs.HistoryTable.Reverse())
        {
            if (o == 't') continue;
            if (last == '\0' || o == last)
            {
                cur++;
                maxRun = Math.Max(maxRun, cur);
            }
            else
            {
                cur = 1;
            }

            last = o;
        }

        return maxRun > s.MaxRunSideAllowedTable + 2;
    }
}