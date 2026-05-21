import type { Log } from "@/service/Dto/Log/Log";

export const mockLogs: Log[] = [
  {
    id: 1,
    json: {
      GlobalMargin: 0,
      HeavyCount: 0,
      Cooldown: 0,
      Rows: {
        "1": {
          PrevMazzo: 0,
          PrevLevel: 0,
          PrevMargine: 0.0,
          PrevStake: 1,
          History: [" "],
          HistoryTable: [],
          RunP: 0,
          ForceToL8Active: false,
          L5ClosedCount: 0,
          HandCount: 1,
          MargineAccum: 0,
          VmLocal20: 0,
          WarmInputs: 1,
          InvalidCount: 0,
          ValidRecovery: 1,
          Disabled: false
        }
      },
      SeenInputs: ["1-0"],
      LastAdvice: {
        "1": {
          TableId: 1,
          LevelIndex: 0,
          StakeUnits: 0.2,
          GlobalMargin: 0,
          StopAtL5: false,
          AuthorizedHeavy: false,
          Reason: "Default L<=4",
          SignalW10: "Green",
          SignalTableW10: "Green",
          HotZone: true,
          TooltipJson: `{
            "STATO_TAVOLO": {
              "logic": "⚙️ Stato tavolo: 🟢 Active",
              "algebra": "Prediction=Safe, StopAtL5=False, Heavy=False"
            },
            "DECISIONE": {
              "logic": "Default L<=4",
              "algebra": "Livello 0, Stake 0.20 (K=0.2), StopAtL5=False, Heavy=False"
            },
            "REGIME": {
              "logic": "Regime MID: neutro, bilanciato",
              "algebra": "Margine globale = 0.00 (High≥160.00, Low≤-160.00) [K=0.2]"
            },
            "ZONA_MAZZO": {
              "logic": "Closed 0-0 🔴 Zone",
              "algebra": "In hot-zone = True"
            },
            "SEMAFORO_W10": {
              "logic": "Semaforo W10 (nostro): Green",
              "algebra": "RunPmaxWin=0 ≤ MaxRunPAllowed=2"
            },
            "SEMAFORO_TAVOLO": {
              "logic": "Semaforo tavolo: Green",
              "algebra": "MaxRunSideAllowedTable=3"
            },
            "HMAX": {
              "logic": "HeavyCount=0 su limite Hmax=2",
              "algebra": "0 < 2 ⇒ gate aperto"
            },
            "COOLDOWN": {
              "logic": "Cooldown impostato=1, residuo=0",
              "algebra": "Cooldown>0 ⇒ attesa"
            },
            "RUN_P": {
              "logic": "Perdite consecutive (RunP): 0",
              "algebra": "0 < 5 ⇒ nessuna protezione attiva"
            },
            "PORTAFOGLIO": {
              "logic": "DebtUnits=0, Override attivi=0",
              "algebra": "TriggerRatio=60% ⇒ override bloccato"
            },
            "K_PARAMETRO": {
              "logic": "Scala attuale: K = 0.2",
              "algebra": "Valori visualizzati in scala K"
            }
          }`,
          HotZoneLabel: "Closed 0-0 🔴 Zone",
          PortfolioDebtUnits: 0,
          HotOverridesActive: 0,
          HotOverridesUsedThisShoe: 0,
          VmLocal20: 0,
          Prediction: "Safe",
          FutureL5Prediction: "Stop L5 previsto (prudenza)",
          TableStatus: "🟢 Active"
        }
      },
      PortfolioDebtUnits: 0,
      HotOverridesActive: 0,
      HotOverridesUsedThisShoe: 0
    },
    datetime: "30/11/2025 16:15:03",
    notes: "",
    margine: 0.0
  }
];
