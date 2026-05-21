using System;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1
{
    /// <summary>
    /// Codice di consiglio / motivazione della scelta di livello.
    /// Serve solo per il debug e per la dashboard.
    /// </summary>
    public enum AdviceCode
{
    None = 0,

    PlainL1 = 10,
    PlainL3 = 11,
    PlainL8 = 12,
    PlainL17 = 13,
    PlainL35 = 14,
    PlainL74 = 15,
    PlainL153 = 16,
    PlainL314 = 17,

    PlainL5_Global = 18,   // ← AGGIUNTO QUI

    StopAtL5_Global = 30,
    StopAtL5_Shoe = 31,
    ForceL6_HotWindow = 50,
    ForceL7_HotWindow = 51,
    ForceL8_HotWindow = 52,
    ForceL8_TimeDebt = 53,
    ForceL8_GlobalVm = 54,
    Cooldown_SlowDown = 70,
    Cooldown_SkipTable = 71,
    BlockAboveL5_TooManyL5 = 90,
    BlockAboveL5_HeavyGate = 91
}


    public sealed class Advice
    {
        public AdviceCode Code { get; }
        public string Reason { get; }

        public Advice(AdviceCode code, string reason)
        {
            Code = code;
            Reason = reason ?? string.Empty;
        }

        public override string ToString()
            => $"{Code}: {Reason}";
    }
}

