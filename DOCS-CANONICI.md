# Documenti canonici — solo DASH2A / Giacomo

> **Scope di questo file:** repository `NuovaDashboard-MarcoTurri` (DASH2A).  
> **Non toccare** documentazione Eugenio / Dashboard 1 (`EUGENIO13`, `EUGENIO11-ARCHITECTURE.md`, repo `TradingDashboard`) — è un altro sistema.  
> **Ultimo aggiornamento:** 2026-06-06

---

## Regola: un report = un documento

Per analisi log produzione DASH2A, indice completo: [`tools/prod-log-export/REPORT-LOG-PROD.md`](tools/prod-log-export/REPORT-LOG-PROD.md)

| Report | Documento canonico | PDF (2026-06-06) |
|--------|-------------------|------------------|
| P&L per zona mazzo (10 gg prod) | `tools/prod-log-export/deck_zone_analysis/deck_zone_prod_report.md` | `Desktop\DASH2A_Analisi_Zona_Mazzo_COMPLETO_2026-06-06.pdf` |
| Simulazione L5 bloccati | `tools/prod-log-export/l5_blocked_analysis/l5_blocked_prod_report.md` | `Desktop\DASH2A_L5_Bloccati_Simulazione_2026-06-06.pdf` |
| Filtro L5 per zona mazzo | `tools/prod-log-export/l5_blocked_analysis/l5_zone_strategy_report.md` | `Desktop\DASH2A_Filtro_L5_Zona_Mazzo_2026-06-06.pdf` |

**Obsoleto (stesso report, campione vecchio):** `deck_zone_analysis/deck_zone_48h_report.md` → usare `deck_zone_prod_report.md`.

---

## Infrastruttura DASH2A

| Agente | Leggere SOLO |
|-----------|----------------|
| **giacom** — dashboard Vue/WebApi/Decisore | **`docs/AGENT-GIACOM.md`** |
| **giacomo1** — GameBot Giacomo | **`docs/AGENT-GIACOMO1.md`** |

File storici (`DASH2A-INFRASTRUCTURE.md`, `DOCS-CANONICI.md` separati) restano in repo — contenuto inlined negli AGENT-*.

**Non leggere per DASH2A:** `tools/eugenio-gamebot/README.md` (solo redirect), `ops/archive/*`, audit generici — salvo task esplicito.

---

## Separazione obbligatoria — due dashboard

| | **DASH2A / Giacomo** | **Dashboard 1 / Eugenio** |
|--|----------------------|---------------------------|
| Repo | `NuovaDashboard-MarcoTurri` | `TradingDashboard` (IIS) |
| Bot sorgente | `tools/eugenio-bot` — agente **`giacomo1`** | `baccarat-bot-main-socket-artifact` — agente **`marco1`** |
| Runtime tipico | `relisegiacomo ok 1.7` (linea giacomo1) | `EUGENIO13` |
| Doc infra | `DASH2A-INFRASTRUCTURE.md` — agente **`giacom`** | `docs/EUGENIO13-GAMEBOT.md` |

**Non modificare, unificare o marcare obsoleti** i file Eugenio da lavoro DASH2A.
