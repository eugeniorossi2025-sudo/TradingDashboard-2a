// ============================================================
// 🔺 EuGenio® – Forma & Metodo
// ProactiveEngine v2.9 – “Soft Validation + WarmStart + Dogma Esteso”
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1
{
    public sealed class ProactiveEngine
    {
        // ============================================================
        // 🔹 CORE DELLA SCUDERIA
        // ============================================================
        private readonly ScuderiaExtensions _scuderia = new ScuderiaExtensions();
        private readonly ProactiveSettings _s;
        private readonly RegiaAdaptiva _regiaDynamic;

        private readonly Dictionary<int, RowState> _rows = new Dictionary<int, RowState>();
        private readonly Dictionary<int, Advice> _lastAdvice = new Dictionary<int, Advice>();
        private readonly Dictionary<int, decimal> _tableMarginsUnits = new Dictionary<int, decimal>();
        private readonly HashSet<string> _seenInputs = new HashSet<string>();

        // 🔸 Stato globale
        private int _globalMarginUnits = 0;
        private int _heavyCount = 0;
        private int _cooldown = 0;

        // 🔸 Stato di portafoglio
        private int _portfolioDebtUnits = 0;
        private int _hotOverridesActive = 0;
        private int _hotOverridesUsedThisShoe = 0;

        // 🔸 PATCH HYBRID – memoria ultimo input per tavolo
        private sealed class LastInput
        {
            public int HandIndex { get; set; }
            public decimal MargineK { get; set; }
            public int MartingalaUi { get; set; }
            public char Esito { get; set; }
        }

        private readonly Dictionary<int, LastInput> _lastInputs = new Dictionary<int, LastInput>();

        // ============================================================
        // 🔹 COSTRUTTORE
        // ============================================================
        public ProactiveEngine(ProactiveSettings settings = null)
        {
            _s = settings ?? new ProactiveSettings();
            _regiaDynamic = new RegiaAdaptiva(_s, 1500, 540);
        }

        public ProactiveSettings GetSettings() => _s;
        public IDictionary<int, RowState> GetRows() => _rows;
        public int GetGlobalMarginUnits() => _globalMarginUnits;

        // ============================================================
        // 🔹 GESTIONE K RUNTIME
        // ============================================================
        public void SetK(double k)
        {
            if (k <= 0) throw new ArgumentException("K must be greater than zero");
            _s.K = k;
        }

        public RegiaAdaptiva GetRegiaAdaptive()
        {
            return _regiaDynamic;
        }

        public double GetK() => _s.K;

        // ============================================================
        // 🔹 L5 SyncStopFix – Stop anticipato prima di L6
        // ============================================================
        private Advice CheckPreemptiveStopL5(int tableId)
        {
            var (hmax, cdn) = GetRegiaParams();
            double capResidua = EstimateResidualCapacityUnits();
            bool triggerDebt = (_portfolioDebtUnits > _s.DebtTriggerRatio * capResidua);
            bool roomClosed = (_heavyCount >= hmax || _cooldown > 0);

            if (roomClosed || triggerDebt)
            {
                _cooldown = Math.Max(_cooldown, cdn);
                return new Advice
                {
                    TableId = tableId,
                    LevelIndex = 4,
                    StopAtL5 = true,
                    AuthorizedHeavy = false,
                    Reason = "🔺 Stop L5 sincronizzato (room closed o debt trigger)",
                    GlobalMargin = Math.Round(_globalMarginUnits * _s.K, 2),
                    SignalW10 = "Green",
                    HotZone = false
                };
            }
            return null;
        }

        public IReadOnlyCollection<char> GetHistory(int tableId)
        {
            if (_rows.TryGetValue(tableId, out var rs))
                return rs.History.ToList();
            return Array.Empty<char>();
        }

        // ============================================================
        // 🔹 PERSISTENZA STATO (JSON)
        // ============================================================
        public string GetJson()
        {
            var data = new StateData
            {
                GlobalMargin = _globalMarginUnits,
                HeavyCount = _heavyCount,
                Cooldown = _cooldown,
                Rows = _rows,
                SeenInputs = _seenInputs.ToList(),
                LastAdvice = _lastAdvice,
                PortfolioDebtUnits = _portfolioDebtUnits,
                HotOverridesActive = _hotOverridesActive,
                HotOverridesUsedThisShoe = _hotOverridesUsedThisShoe
            };
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        public void SaveStateToFile(string path) => File.WriteAllText(path, GetJson());

        public void LoadStateFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            var data = JsonSerializer.Deserialize<StateData>(json);
            if (data == null) return;

            _globalMarginUnits = data.GlobalMargin;
            _heavyCount = data.HeavyCount;
            _cooldown = data.Cooldown;

            _rows.Clear();
            foreach (var kv in data.Rows) _rows[kv.Key] = kv.Value;

            _seenInputs.Clear();
            foreach (var sKey in data.SeenInputs) _seenInputs.Add(sKey);

            _lastAdvice.Clear();
            foreach (var kv in data.LastAdvice) _lastAdvice[kv.Key] = kv.Value;

            _portfolioDebtUnits = data.PortfolioDebtUnits;
            _hotOverridesActive = data.HotOverridesActive;
            _hotOverridesUsedThisShoe = data.HotOverridesUsedThisShoe;
        }

        public void LoadStateFromFile(string path)
        {
            if (!File.Exists(path)) return;
            LoadStateFromJson(File.ReadAllText(path));
        }

        public class StateData
        {
            public int GlobalMargin { get; set; }
            public int HeavyCount { get; set; }
            public int Cooldown { get; set; }
            public Dictionary<int, RowState> Rows { get; set; } = new Dictionary<int, RowState>();
            public List<string> SeenInputs { get; set; } = new List<string>();
            public Dictionary<int, Advice> LastAdvice { get; set; } = new Dictionary<int, Advice>();
            public int PortfolioDebtUnits { get; set; }
            public int HotOverridesActive { get; set; }
            public int HotOverridesUsedThisShoe { get; set; }
        }

        // ============================================================
        // 🔹 UTILITY REGIA
        // ============================================================
        private (int hmax, int cdn) GetRegiaParams()
        {
            if (_globalMarginUnits >= _s.HighThresh) return (_s.HmaxHigh, _s.CooldownHigh);
            if (_globalMarginUnits <= _s.LowThresh) return (_s.HmaxLow, _s.CooldownLow);
            return (_s.HmaxMid, _s.CooldownMid);
        }

        private bool InHotZone(int handNo, bool bHotZone)
        {
            if (!bHotZone) return false;
            foreach (var z in _s.HotZones)
                if (handNo >= z.start && handNo <= z.end) return true;
            return false;
        }

        private string GetHotZoneLabel(int handNo)
        {
            foreach (var z in _s.HotZones)
                if (handNo >= z.start && handNo <= z.end)
                    return $"Closed {z.start}-{z.end} 🔴 Zone";
            return $"Open Zone (hand {handNo}) ⚪";
        }

        private double EstimateResidualCapacityUnits()
        {
            int activeTables = Math.Max(1, _rows.Count);
            return _s.MeanUnitsPerHandPerTable * _s.EstimatedHandsLeftPerTable * activeTables;
        }

        private int GetMaxRunP(RowState rs)
        {
            int max = 0, cur = 0;
            foreach (var o in rs.History)
            {
                if (o == 'P') { cur++; if (cur > max) max = cur; }
                else cur = 0;
            }
            return max;
        }

        // ============================================================
        // 🔺 FEED AND DECIDE – con validazione morbida (PATCH HYBRID)
        // ============================================================
        public Advice FeedAndDecide(int tableId, int handIndexMazzo, decimal margineK, int martingalaUi,
                            bool bSignalW10, bool bHotZone,
                            char esito, double totalElapsedMinutes, int totaltables)
        {
            // ============================================================
            // 1) Recupero stato riga (o creazione) - NON incremento warm qui
            // ============================================================
            if (!_rows.TryGetValue(tableId, out var rsTmp))
                _rows[tableId] = rsTmp = new RowState();

            // ============================================================
            // 2) Validazione soft dei campi MINIMALI
            // ============================================================
            bool invalid = false;
            if (tableId <= 0 || handIndexMazzo <= 0) invalid = true;
            if (double.IsNaN((double)margineK) || double.IsInfinity((double)margineK)) invalid = true;
            if (martingalaUi < 1) invalid = true;

            // ============================================================
  // ============================================================
// 🔥 PATCH W10 + DUPLICATI HYBRID (versione identica al tuo file)
// ============================================================
string key = $"{tableId}-{handIndexMazzo}";
bool keySeen = _seenInputs.Contains(key);

if (keySeen &&
    _lastAdvice.TryGetValue(tableId, out var lastAdv) &&
    _lastInputs.TryGetValue(tableId, out var lastIn))
{
    bool sameHand       = lastIn.HandIndex    == handIndexMazzo;
    bool sameMargine    = lastIn.MargineK     == margineK;
    bool sameMartingala = lastIn.MartingalaUi == martingalaUi;
    bool sameEsito      = lastIn.Esito        == esito;

    if (sameHand && sameMargine && sameMartingala && sameEsito)
    {
        // 🔥 PATCH W10 —— aggiorno SEMPRE W10 anche quando l’input è duplicato
        if (esito != 'T')
        {
            rsTmp.History.Enqueue(esito);
            while (rsTmp.History.Count > _s.WindowW10)
                rsTmp.History.Dequeue();
        }

        EnqueueTableOutcome(rsTmp, char.ToLower(esito));

        // 🔁 DUPLICATO REALE → riuso l’ultimo advice
        return lastAdv;
    }
}
else
{
    // Prima volta che vediamo questa mano per questo tavolo
    _seenInputs.Add(key);
}


            // ============================================================
            // 4) Warm-up counter SOLO su input nuovi (non duplicati reali)
            //    - Qui consideriamo "nuovo" tutto ciò che non è stato intercettato sopra
            // ============================================================
            rsTmp.WarmInputs++;
            bool inGrace = rsTmp.WarmInputs <= 3;

            // Eccezione: se siamo in Dogma (ForceToL8Active) e livello >= L6,
            // ignora invalid “soft” per non bloccare la marcia.
            if (invalid && rsTmp.ForceToL8Active && martingalaUi >= 6)
                invalid = false;

            // Durante i primi 3 input validi, rilassa la validazione per evitare falsi stop allo start
            if (invalid && inGrace)
                invalid = false;

            // Se ANCORA invalid → incrementa metrica sporco e applica possibile disable temporaneo
            if (invalid)
            {
                rsTmp.InvalidCount++;
                rsTmp.ValidRecovery = 0;

                if (rsTmp.InvalidCount >= 5)
                {
                    rsTmp.Disabled = true;
                    return new Advice
                    {
                        TableId = tableId,
                        Reason = "Tavolo disabilitato (troppi input invalidi)",
                        Prediction = "Disabled",
                        StopAtL5 = true,
                        AuthorizedHeavy = false
                    };
                }

                return new Advice
                {
                    TableId = tableId,
                    Reason = "Input invalido ignorato",
                    Prediction = "Safe",
                    StopAtL5 = false,
                    AuthorizedHeavy = false
                };
            }

            // Qui l’input è OK → recupero
            rsTmp.ValidRecovery++;
            if (rsTmp.ValidRecovery >= 3)
            {
                rsTmp.Disabled = false;
                rsTmp.InvalidCount = 0;
            }

            // ============================================================
            // 5) Conversione K → UNITÀ, stato globale e aggiornamento Regia
            // ============================================================
            decimal k = (decimal)Math.Max(0.0000001, _s.K);
            decimal margineUnits = margineK / k;

            _tableMarginsUnits[tableId] = margineUnits;
            _globalMarginUnits = (int)Math.Round(_tableMarginsUnits.Values.Sum());

            _regiaDynamic.UpdateDynamicParameters(_globalMarginUnits, totalElapsedMinutes, totaltables);

            // ============================================================
            // 6) STOP MISSIONE (stop-win globale)
            // ============================================================
            if (_regiaDynamic.ShouldStopMission())
            {
                var stop = new Advice
                {
                    TableId = tableId,
                    LevelIndex = 0,
                    StakeUnits = 0.0,
                    GlobalMargin = Math.Round(_globalMarginUnits * _s.K, 2),
                    AuthorizedHeavy = false,
                    StopAtL5 = true,
                    Reason = "STOP-WIN: missione completata",
                    SignalW10 = "Green",
                    HotZone = false,
                    HotZoneLabel = GetHotZoneLabel(handIndexMazzo),
                    Prediction = "Stop Missione"
                };
                _lastAdvice[tableId] = stop;

                // Aggiorno ultimo input per coerenza con la mano
                _lastInputs[tableId] = new LastInput
                {
                    HandIndex = handIndexMazzo,
                    MargineK = margineK,
                    MartingalaUi = martingalaUi,
                    Esito = esito
                };

                return stop;
            }

            // ============================================================
            // 7) Early SyncStopFix – se feed segnala L5, valuta stop prima di tutto
            // ============================================================
            if (martingalaUi == 5)
            {
                var early = CheckPreemptiveStopL5(tableId);
                if (early != null)
                {
                    early.Reason = "🔺 Stop L5 anticipato (EarlySync)";
                    early.HotZone = false;
                    early.SignalW10 = "Green";
                    early.HotZoneLabel = GetHotZoneLabel(handIndexMazzo);
                    early.Prediction = "Stop L5 (EarlySync)";
                    _lastAdvice[tableId] = early;

                    _lastInputs[tableId] = new LastInput
                    {
                        HandIndex = handIndexMazzo,
                        MargineK = margineK,
                        MartingalaUi = martingalaUi,
                        Esito = esito
                    };

                    return early;
                }
            }

            // ============================================================
            // 8) Stato riga (ora possiamo usare rs = rsTmp)
            // ============================================================
            var rs = rsTmp;

            int levelIdx = OutcomeInferer.ToLevelIndex(martingalaUi);
            int stakeUnitsNow = _s.Levels[Math.Min(levelIdx, _s.Levels.Length - 1)];
            double stakeShownK = stakeUnitsNow * _s.K;

            // Storico outcome “scarno” (P/B/T)
            char outcome = esito;
            if (outcome != 'T')
            {
                rs.History.Enqueue(outcome);
                while (rs.History.Count > _s.WindowW10) rs.History.Dequeue();
            }
            EnqueueTableOutcome(rs, char.ToLower(esito));

            if (outcome == 'P') rs.RunP++;
            else if (outcome == 'B') rs.RunP = 0;

            // Vm locale ogni 20 mani
            rs.HandCount++;
            rs.MargineAccum += (double)margineK;
            if (rs.HandCount % 20 == 0)
            {
                rs.VmLocal20 = rs.MargineAccum / 20.0;
                rs.MargineAccum = 0.0;
            }

            // Uscita da heavy con win → spegni Dogma
            bool exitingHeavy = (outcome == 'B' && rs.PrevLevel >= 5 && levelIdx == 0);
            if (exitingHeavy && rs.ForceToL8Active)
            {
                rs.ForceToL8Active = false;
                if (_hotOverridesActive > 0) _hotOverridesActive--;
            }

            if (_cooldown > 0) _cooldown--;
            var regia = GetRegiaParams();

            // ============================================================
            // 9) Advice base
            // ============================================================
            var adv = new Advice
            {
                TableId = tableId,
                LevelIndex = levelIdx,
                StakeUnits = Math.Round(stakeShownK, 2),
                StopAtL5 = false,
                AuthorizedHeavy = false,
                SignalW10 = "Green",
                SignalTableW10 = "Green",
                HotZone = InHotZone(handIndexMazzo, bHotZone),
                GlobalMargin = Math.Round(_globalMarginUnits * _s.K, 2),
                Reason = "Default L<=4",
                HotZoneLabel = GetHotZoneLabel(handIndexMazzo),
                PortfolioDebtUnits = _portfolioDebtUnits,
                HotOverridesActive = _hotOverridesActive,
                HotOverridesUsedThisShoe = _hotOverridesUsedThisShoe,
                VmLocal20 = rs.VmLocal20,
                Prediction = "Safe"
            };

            bool inHot = adv.HotZone;
            bool severeRed = IsSevereRedTableSignal(rs);
            bool hmaxClosed = (_heavyCount >= regia.hmax);
            bool roomCooldown = (_cooldown > 0);

            // ============================================================
            // 10) Early Dogma: se già in marcia → non fermare
            // ============================================================
            if (rs.ForceToL8Active && levelIdx >= 4)
            {
                adv.AuthorizedHeavy = true;
                adv.StopAtL5 = false;
                adv.Prediction = "Dogma L8";
                adv.Reason = $"Dogma attivo → L{levelIdx + 1} autorizzata";
            }

            // ============================================================
            // 11) Hard Dogma fino a L8 (nessun paracadute)
            // ============================================================
            if (levelIdx >= 5 && rs.ForceToL8Active)
            {
                _scuderia.ApplySyncDelay(_s);
                adv.AuthorizedHeavy = true;
                adv.Reason = $"Dogma attivo → marcia L{levelIdx + 1}";
                _heavyCount++;
                _cooldown = Math.Max(_cooldown, regia.cdn);
                adv.Prediction = "Dogma L8";

                FinalizeRow(rs, handIndexMazzo, levelIdx, margineUnits, stakeUnitsNow, tableId, adv);
                _scuderia.ApplyHeavyDecay(ref _heavyCount, ref _cooldown, _s,
                    adv.AuthorizedHeavy && adv.LevelIndex >= 5);

                _lastInputs[tableId] = new LastInput
                {
                    HandIndex = handIndexMazzo,
                    MargineK = margineK,
                    MartingalaUi = martingalaUi,
                    Esito = esito
                };

                return adv;
            }

            // ============================================================
            // 12) Gate L5 — Dogma Esteso
            // ============================================================
            if (levelIdx == 4 && !rs.ForceToL8Active)
            {
                double capResidua = EstimateResidualCapacityUnits();
                bool triggerDebt = (_portfolioDebtUnits > _s.DebtTriggerRatio * capResidua);
                bool canOverride = (_hotOverridesActive < _s.MaxHotOverridesConcurrent)
                                && (_hotOverridesUsedThisShoe < _s.MaxHotOverridesPerShoe);

                if (inHot || severeRed)
                {
                    adv.StopAtL5 = true;
                    adv.Reason = "Stop L5: hot/rosso severo";
                    _portfolioDebtUnits += _s.L5LossUnits;
                    rs.L5ClosedCount++;
                    adv.Prediction = "Stop L5";
                }
                else if (rs.VmLocal20 > 0 && !roomCooldown && !hmaxClosed)
                {
                    _scuderia.ApplySyncDelay(_s);
                    adv.AuthorizedHeavy = true;
                    adv.StopAtL5 = false;
                    adv.Reason = $"Dogma esteso: L5 fisiologico (Vm20 {rs.VmLocal20:F2}) → L6–L8";
                    _heavyCount++;
                    _cooldown = regia.cdn;
                    rs.ForceToL8Active = true;
                    adv.Prediction = "L6 autorizzata";
                }
                else if (triggerDebt && !roomCooldown && !hmaxClosed && canOverride && rs.RunP < 5)
                {
                    _scuderia.ApplySyncDelay(_s);
                    adv.AuthorizedHeavy = true;
                    adv.StopAtL5 = false;
                    adv.Reason = "Override HOT L5: debt trigger → Dogma avviato";
                    _heavyCount++;
                    _cooldown = regia.cdn;
                    if (canOverride) { _hotOverridesActive++; _hotOverridesUsedThisShoe++; }
                    rs.ForceToL8Active = true;
                    adv.Prediction = "L6 autorizzata";
                }
                else
                {
                    adv.StopAtL5 = true;
                    adv.AuthorizedHeavy = false;
                    adv.Reason = "Stop L5: default prudente";
                    _portfolioDebtUnits += _s.L5LossUnits;
                    rs.L5ClosedCount++;
                    adv.Prediction = "Stop L5";
                }
            }

            // ============================================================
            // 13) Tooltip e finalizzazione
            // ============================================================
            try
            {
                var rp = GetRegiaParams();
                adv.TooltipJson = TooltipBuilder.BuildTooltipJson(
                    adv, _s, handIndexMazzo, rs.RunP, GetMaxRunP(rs),
                    _heavyCount, rp.hmax, rp.cdn, _cooldown);
            }
            catch (Exception ex)
            {
                adv.TooltipJson = $"{{\"error\":\"Tooltip generation failed: {ex.Message}\"}}";
            }

            FinalizeRow(rs, handIndexMazzo, levelIdx, margineUnits, stakeUnitsNow, tableId, adv);
            _scuderia.ApplyHeavyDecay(ref _heavyCount, ref _cooldown, _s,
                adv.AuthorizedHeavy && adv.LevelIndex >= 5);

            // 🔁 aggiorno memoria ultimo input per la PATCH HYBRID
            _lastInputs[tableId] = new LastInput
            {
                HandIndex = handIndexMazzo,
                MargineK = margineK,
                MartingalaUi = martingalaUi,
                Esito = esito
            };

            return adv;
        }

        // ============================================================
        // 🔹 Supporto finale
        // ============================================================
        private void FinalizeRow(RowState rs, int handIndexMazzo, int levelIdx,
                                 decimal margineUnits, int stakeUnitsNow, int tableId, Advice adv)
        {
            rs.PrevMazzo = handIndexMazzo;
            rs.PrevLevel = levelIdx;
            rs.PrevMargine = margineUnits;
            rs.PrevStake = stakeUnitsNow;
            _lastAdvice[tableId] = adv;
        }

        private void EnqueueTableOutcome(RowState rs, char side)
        {
            if (side != 'p' && side != 'b' && side != 't') return;
            rs.HistoryTable.Enqueue(side);
            while (rs.HistoryTable.Count > _s.WindowW10) rs.HistoryTable.Dequeue();
        }

        private bool IsSevereRedTableSignal(RowState rs)
        {
            if (rs.HistoryTable == null || rs.HistoryTable.Count == 0) return false;
            int cur = 0, maxRun = 0; char last = '\0';
            foreach (var o in rs.HistoryTable.Reverse())
            {
                if (o == 't') continue;
                if (last == '\0' || o == last)
                {
                    cur++;
                    maxRun = Math.Max(maxRun, cur);
                }
                else cur = 1;
                last = o;
            }
            return (maxRun > _s.MaxRunSideAllowedTable + 2);
        }
    }
}
