using System.Collections.Generic;
using System.Text.Json;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1;

public static class TooltipBuilder
{
    public static string BuildTooltipJson(
        Advice adv,
        ProactiveSettings s,
        int handIndexMazzo,
        int runP,
        int runPmaxWin,
        int heavyCount,
        int hmaxApplied,
        int cooldownApplied,
        int cooldownRemaining)
    {
        var notes = new Dictionary<string, object>();

        // --- STATO GENERALE ---
        notes["STATO_TAVOLO"] = new
        {
            logic = $"⚙️ Stato tavolo: {GetStatusFromPrediction(adv.Prediction)}",
            algebra = $"Prediction={adv.Prediction}, StopAtL5={adv.StopAtL5}, Heavy={adv.AuthorizedHeavy}"
        };

        // --- DECISIONE ---
        notes["DECISIONE"] = new
        {
            logic = adv.Reason,
            algebra =
                $"Livello {adv.LevelIndex}, Stake {adv.StakeUnits:F2} (K={s.K}), StopAtL5={adv.StopAtL5}, Heavy={adv.AuthorizedHeavy}"
        };

        // --- REGIME (soglie mostrate in K per coerenza dashboard) ---
        notes["REGIME"] = new
        {
            logic = _GetRegimeDescription(adv.GlobalMargin, s),
            algebra = $"Margine globale = {adv.GlobalMargin:F2} " +
                      $"(High≥{s.HighThresh * s.K:F2}, Low≤{s.LowThresh * s.K:F2}) [K={s.K}]"
        };

        // --- ZONA MAZZO ---
        notes["ZONA_MAZZO"] = new
        {
            logic = adv.HotZoneLabel,
            algebra = $"In hot-zone = {adv.HotZone}"
        };

        // --- SEMAFORO W10 ---
        notes["SEMAFORO_W10"] = new
        {
            logic = $"Semaforo W10 (nostro): {adv.SignalW10}",
            algebra = $"RunPmaxWin={runPmaxWin} ≤ MaxRunPAllowed={s.MaxRunPAllowed}"
        };

        // --- SEMAFORO TAVOLO ---
        notes["SEMAFORO_TAVOLO"] = new
        {
            logic = $"Semaforo tavolo: {adv.SignalTableW10}",
            algebra = $"MaxRunSideAllowedTable={s.MaxRunSideAllowedTable}"
        };

        // --- HMAX ---
        notes["HMAX"] = new
        {
            logic = $"HeavyCount={heavyCount} su limite Hmax={hmaxApplied}",
            algebra = $"{heavyCount} < {hmaxApplied} ⇒ gate aperto"
        };

        // --- COOLDOWN ---
        notes["COOLDOWN"] = new
        {
            logic = $"Cooldown impostato={cooldownApplied}, residuo={cooldownRemaining}",
            algebra = $"Cooldown>0 ⇒ attesa, altrimenti via libera"
        };

        // --- RUN P ---
        notes["RUN_P"] = new
        {
            logic = $"Perdite consecutive (RunP): {runP}",
            algebra = $"{runP} < 5 ⇒ nessuna protezione attiva"
        };

        // --- PORTAFOGLIO ---
        notes["PORTAFOGLIO"] = new
        {
            logic = $"DebtUnits={adv.PortfolioDebtUnits}, Override attivi={adv.HotOverridesActive}",
            algebra =
                $"TriggerRatio={s.DebtTriggerRatio * 100:F1}% ⇒ override={(adv.AuthorizedHeavy ? "autorizzato" : "bloccato")}"
        };

        // --- K INFO ---
        notes["K_PARAMETRO"] = new
        {
            logic = $"Scala attuale: K = {s.K}",
            algebra = $"Tutti i valori visualizzati sono in scala K (unità×K)"
        };

        return JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
    }

    // ============================================================
    // Helper per la dashboard: mappa predizioni → stato visuale
    // ============================================================
    private static string GetStatusFromPrediction(string prediction)
    {
        if (string.IsNullOrWhiteSpace(prediction)) return "🟢 Active";
        prediction = prediction.ToLowerInvariant();

        if (prediction.Contains("hold") || prediction.Contains("pause"))
            return "⚫ Hold";
        if (prediction.Contains("recover") || prediction.Contains("rejoin"))
            return "🟡 Rejoin";
        if (prediction.Contains("stop"))
            return "🔴 Stop";
        if (prediction.Contains("dogma"))
            return "🔺 Dogma L8";
        return "🟢 Active";
    }

    private static string _GetRegimeDescription(double marginK, ProactiveSettings s)
    {
        var highK = s.HighThresh * s.K;
        var lowK = s.LowThresh * s.K;
        if (marginK >= highK) return "Regime ALTO: prudente (raffreddamento)";
        if (marginK <= lowK) return "Regime BASSO: aggressivo (espansione)";
        return "Regime MID: neutro, bilanciato";
    }
}