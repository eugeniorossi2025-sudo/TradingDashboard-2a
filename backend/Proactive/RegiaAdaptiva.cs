// ============================================================
// 🔺 EuGenio® – Forma & Metodo
// Regia Astronave Adaptiva v2.9.1 – “Missione Proporzionale + Warm-Up Fisso (10 min)”
// - Warm-up sempre fisso a 10 minuti reali
// - Target configurabile, telemetria dashboard integrata
// - Compatibile con ProactiveEngine v2.8+
// ============================================================

using System;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1;

public sealed class RegiaAdaptiva
{
    private readonly ProactiveSettings _s;

    // ============================================================
    // 🔹 Parametri missione dinamici (default 1500u / 540min / 10 tavoli)
    // ============================================================
    private int _targetUnitsTotal = 900;
    private int _targetMinutesTotal = 480;
    private int _targetTables = 10;
    private int _targetUnitsPerTable = 90;

    private bool _missionCompleted = false;

    // Efficienza reale (η): tempo utile/tempo teorico
    private readonly double _efficiencyFactor = 0.25;

    // Target Vm globale (in UNITÀ/min)
    public double VmTargetGlobal { get; private set; } = 0.0;

    public bool ShouldStopMission() => _missionCompleted;

    public RegiaAdaptiva(ProactiveSettings settings, int targetMargin = 500, int targetMinutes = 180)
    {
        _s = settings;
        _targetUnitsTotal = targetMargin;
        _targetMinutesTotal = targetMinutes;
        _targetTables = 10;
        _targetUnitsPerTable = Math.Max(1, targetMargin / Math.Max(1, _targetTables));
    }

    // ============================================================
    // 🔺 Impostazione missione proporzionale
    // ============================================================
    public void SetMissionParameters(int targetUnits, int totalMinutes, int totalTables)
    {
        _targetUnitsTotal = Math.Max(100, targetUnits);
        _targetMinutesTotal = Math.Max(60, totalMinutes);
        _targetTables = Math.Max(1, totalTables);
        _targetUnitsPerTable = (int)Math.Round((double)_targetUnitsTotal / _targetTables);
        _missionCompleted = false;
    }

    public (int UnitsTarget, int MinutesTarget, int TablesTarget, double VmTarget) GetMissionInfo()
    {
        return (_targetUnitsTotal, _targetMinutesTotal, _targetTables, VmTargetGlobal);
    }

    // ============================================================
    // 🔺 Regia adattiva dinamica
    // ============================================================
    public void UpdateDynamicParameters(double currentMarginUnits, double elapsedMinutes, int TavoliAttivi)
    {
        // ---------- Warm-Up Fisso (10 minuti) ----------
        var warmUpMinutes = 10.0;
        if (elapsedMinutes < warmUpMinutes)
        {
            _s.LowThresh = -800;
            _s.HighThresh = 800;
            _s.DebtTriggerRatio = 0.60;
            _s.HmaxLow = 2;
            _s.HmaxMid = 2;
            _s.HmaxHigh = 1;
            _s.CooldownLow = 1;
            _s.CooldownMid = 1;
            _s.CooldownHigh = 1;

            VmTargetGlobal = (double)_targetUnitsTotal / _targetMinutesTotal;
            return;
        }

        // ---------- Calcolo missione dinamica ----------
        var targetTotalAdj = _targetUnitsTotal * Math.Max(1, TavoliAttivi) / Math.Max(1, _targetTables) *
                             _efficiencyFactor;
        var missionMinutes = _targetMinutesTotal * Math.Max(1, TavoliAttivi) / Math.Max(1, _targetTables) * 1.2;
        var vmTarget = targetTotalAdj / Math.Max(1.0, missionMinutes);
        VmTargetGlobal = vmTarget;

        var vm = currentMarginUnits / Math.Max(1.0, elapsedMinutes);
        var progress = targetTotalAdj <= 0 ? 0 : currentMarginUnits / targetTotalAdj;

        // ---------- Stop-Win ----------
        if (currentMarginUnits >= targetTotalAdj && !_missionCompleted)
        {
            _missionCompleted = true;

            _s.LowThresh = 0;
            _s.HighThresh = 0;
            _s.HmaxLow = 0;
            _s.HmaxMid = 0;
            _s.HmaxHigh = 0;
            _s.CooldownLow = 9999;
            _s.CooldownMid = 9999;
            _s.CooldownHigh = 9999;
            return;
        }

        // ---------- Fase intermedia ----------
        if (progress >= 0.50 && elapsedMinutes >= missionMinutes * 0.30)
        {
            _s.LowThresh = -800;
            _s.HighThresh = 600;
            _s.DebtTriggerRatio = 0.65;
            _s.HmaxLow = Math.Max(2, TavoliAttivi / 4);
            _s.HmaxMid = 1;
            _s.HmaxHigh = 0;
            _s.CooldownLow = 1;
            _s.CooldownMid = 2;
            _s.CooldownHigh = 2;
            return;
        }

        // ---------- Regolazione dinamica ----------
        if (vm < vmTarget)
        {
            // Sistema “under”: serve spinta
            _s.LowThresh = -1000;
            _s.DebtTriggerRatio = 0.55;
            _s.HmaxLow = Math.Max(5, TavoliAttivi / 2 + 1);
            _s.CooldownLow = 1;
        }
        else if (vm > vmTarget * 1.5)
        {
            // Sistema “over”: raffredda
            _s.HighThresh = 1000;
            _s.HmaxHigh = 1;
            _s.CooldownHigh = 2;
            _s.DebtTriggerRatio = 0.70;
        }
        else
        {
            // Equilibrio
            _s.LowThresh = -1000;
            _s.HighThresh = 800;
            _s.DebtTriggerRatio = 0.60;
            _s.HmaxMid = Math.Max(2, TavoliAttivi / 5);
            _s.CooldownMid = 1;
        }
    }

    // ============================================================
    // 🔺 Snapshot per Dashboard
    // ============================================================
    public MissionSnapshot GetDashboardSnapshot(
        double currentMarginUnits,
        double elapsedMinutes,
        int tavoliAttivi,
        double k)
    {
        double targetUnitsAdj = _targetUnitsTotal * Math.Max(1, tavoliAttivi) / Math.Max(1, _targetTables);
        double missionMinutesAdj = _targetMinutesTotal * Math.Max(1, tavoliAttivi) / Math.Max(1, _targetTables);
        var vmTargetUnits = targetUnitsAdj / Math.Max(1.0, missionMinutesAdj);

        // Warm-Up Fisso (10 min)
        var warmUpMinutes = 10.0;
        var warmUpActive = elapsedMinutes < warmUpMinutes;

        var targetEuro = targetUnitsAdj * k;
        var vmTargetEuro = vmTargetUnits * k;

        var achievementPct = 0.0;
        if (targetEuro > 0)
        {
            var marginEuro = currentMarginUnits * k;
            achievementPct = marginEuro / targetEuro * 100.0;
        }

        return new MissionSnapshot
        {
            TargetUnitsAdj = targetUnitsAdj,
            MissionMinutesAdj = missionMinutesAdj,
            VmTargetUnits = vmTargetUnits,

            TargetEuro = targetEuro,
            VmTargetEuro = vmTargetEuro,

            WarmUpMinutes = warmUpMinutes,
            WarmUpActive = warmUpActive,

            AchievementPercent = Math.Round(achievementPct, 2),
            K = k,
            TavoliAttivi = Math.Max(1, tavoliAttivi),
            MissionCompleted = _missionCompleted
        };
    }

    // ============================================================
    // 🔺 Costruzione risultato per dashboard
    // ============================================================
    public ValutazioneRisultato BuildValutazione(MissionSnapshot snap, double currentMarginEuro, double elapsedMinutes)
    {
        var vm = currentMarginEuro / Math.Max(1.0, elapsedMinutes);

        if (snap.WarmUpActive)
            return new ValutazioneRisultato
            {
                Message = $"Warm-Up ({Math.Round(elapsedMinutes, 1)} / {snap.WarmUpMinutes:F0} min)",
                VmValue = 0,
                Color = "gray"
            };

        var ratio = vm / Math.Max(0.000001, snap.VmTargetEuro);
        string msg, col;

        if (ratio < 0.9)
        {
            msg = $"Dogma – Vm {vm:F2} €/min (under rhythm)";
            col = "red";
        }
        else if (ratio > 1.1)
        {
            msg = $"Protection – Vm {vm:F2} €/min (forward)";
            col = "yellow";
        }
        else
        {
            msg = $"Neutral – Vm {vm:F2} €/min (aligned)";
            col = "green";
        }

        msg +=
            $" | Tavoli={snap.TavoliAttivi} | Target={snap.TargetEuro:F0} | VmTarget={snap.VmTargetEuro:F2} | K={snap.K:F2}";

        return new ValutazioneRisultato
        {
            Message = msg,
            VmValue = vm,
            Color = col
        };
    }

    // ============================================================
    // 🔹 DTO: snapshot missione per dashboard
    // ============================================================
    public sealed class MissionSnapshot
    {
        public double TargetUnitsAdj { get; set; }
        public double MissionMinutesAdj { get; set; }
        public double VmTargetUnits { get; set; }

        public double TargetEuro { get; set; }
        public double VmTargetEuro { get; set; }

        public double WarmUpMinutes { get; set; }
        public bool WarmUpActive { get; set; }

        public double AchievementPercent { get; set; }
        public double K { get; set; }
        public int TavoliAttivi { get; set; }
        public bool MissionCompleted { get; set; }
    }
}

// ============================================================
// 🔹 Compatibilità con dashboard
// ============================================================
public sealed class ValutazioneRisultato
{
    public string Message { get; set; }
    public double VmValue { get; set; }
    public string Color { get; set; }
}