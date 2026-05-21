using System;
using System.Collections.Generic;

// ===== [SCUDERIA ADD] Estensioni non invasive =====
namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1;

public sealed class ScuderiaExtensions
{
    private int _handsSinceLastHeavy = 0;
    private readonly Queue<DateTime> _recentHeavyTimestamps = new();

    // ============================================================
    // 🔺 ApplySyncDelay — piccola pausa di sincronizzazione
    // ============================================================
    public void ApplySyncDelay(ProactiveSettings s)
    {
        if (s != null && s.SyncDelayMs > 0)
            System.Threading.Thread.Sleep(s.SyncDelayMs);
    }

    // ============================================================
    // 🔺 ApplyHeavyDecay — decay controllato e stabile
    // ============================================================
    public void ApplyHeavyDecay(ref int heavyCount, ref int cooldown, ProactiveSettings s, bool enteredHeavy)
    {
        if (enteredHeavy)
        {
            _handsSinceLastHeavy = 0;
            return;
        }

        _handsSinceLastHeavy++;
        var threshold = s != null ? s.HeavyDecayAfterHands : 4;

        // 🔺 FIX: evita che il decay riduca heavyCount mentre siamo ancora in cooldown
        if (cooldown == 0 && heavyCount > 0 && _handsSinceLastHeavy >= threshold)
        {
            heavyCount--;
            _handsSinceLastHeavy = 0;
        }
        else if (heavyCount == 0 && cooldown > 0)
        {
            // 🔺 Micro-lock di stabilità:
            // se siamo in cooldown ma senza heavy attivi, impedisce reset prematuro del contatore
            _handsSinceLastHeavy = 0;
        }
    }

    // ============================================================
    // 🔺 AllowHeavyGlobal — controllo limite globale heavy
    // ============================================================
    public bool AllowHeavyGlobal(ProactiveSettings s)
    {
        if (s == null) return true;

        var cutoff = DateTime.UtcNow.AddSeconds(-s.GlobalHeavyCapWindow);

        // pulizia timestamp obsoleti
        while (_recentHeavyTimestamps.Count > 0 && _recentHeavyTimestamps.Peek() < cutoff)
            _recentHeavyTimestamps.Dequeue();

        // controllo limite attuale
        if (_recentHeavyTimestamps.Count >= s.GlobalHeavyCap)
            return false;

        _recentHeavyTimestamps.Enqueue(DateTime.UtcNow);
        return true;
    }
}
// ===== [END SCUDERIA ADD] =====