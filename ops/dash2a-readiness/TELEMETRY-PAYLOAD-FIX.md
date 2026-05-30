# Telemetry payload — root cause e fix strutturale (DASH2A)

**Stato:** audit completato · fix non ancora deployato  
**Repo:** `NuovaDashboard-MarcoTurri` / `TradingDashboard-2a`  
**Data audit:** 2026-05-30

---

## Root cause (confermata)

| Elemento | Valore |
|----------|--------|
| Colonna DB | `dbo.Statistiche.TELEMETRY` → `NVARCHAR(4000)` (invariata da legacy) |
| Commit regressione | **`1324ade`** (2026-05-27 19:29) + cluster stesso giorno |
| Prima (`454becd`, 17 prop/bot) | 2 bot ≈ **1400** char → OK |
| Dopo (`1324ade`+, 62 prop/bot) | 2 bot ≈ **5200** char → troncato a 4000 |
| Effetto UI | `JSON.parse(rawTelemetry)` fallisce → Global Statistics e Control Room vuoti |
| Non è | storico missioni, accumulo tick, Dashboard 1 |

---

## 1. Verifica commit `1324ade`

```
commit 1324ade2eafcbda47680ca01ba54846559ccb1e0
Author: eugeniorossi2025-sudo
Date:   Wed May 27 19:29:31 2026 +0200
Subject: fix: normalize security filter telemetry per bot hand

File toccati:
  decision-engine/Decisore/Engine/Telemetry.cs       (+17 righe in questo commit)
  decision-engine/Decisore/Engine/ProactiveEngine.cs (+124 righe — popolamento campi)
  backend/WebApi/Controllers/DashboardController.cs  (+17 righe DTO)
  frontend/src/components/dashboard/StatsWidget.vue  (+46 righe pannello dettaglio)
```

**Nota:** l’espansione **17 → 62 proprietà/bot** è cumulativa in **6 commit del 27/05**:

| Commit | Aggiunta principale |
|--------|---------------------|
| `454becd` | Nascita `SecurityFilterByBot` (17 campi) |
| `a0a04a7` | Min/max hand delta, primi campi L6 |
| `00d33df` | Config SF serializzata a root (+7 campi telemetry) |
| `da496a1` | Refinement UI |
| **`1324ade`** | **Normalizzazione per mano: L6/L8 hands, auth→L8, LastL6Authorization completo** |
| `b31c2c6` | `LastTwoHandDeltaSeconds[]`, `RapidL5TriggerActive` |

La soglia **2 bot oltre 4000** si supera con schema **`1324ade`** (~5098 char simulati; prod troncato a 4000).

Campi aggiunti cumulativamente `454becd → 1324ade` (45 nuovi nomi in `SecurityFilterBotTelemetry`):

- Timing mani: `MinHandDeltaSeconds`, `MaxHandDeltaSeconds`
- L6: `L6PlayedCount`, `Last/Avg/Min/MaxL6DeltaSeconds`, `Last/Avg/Min/MaxL6DeltaHands`, `L6DeltaSamples`, `LastL6PlayedAtUtc`, `LastL6PlayedPBHands`
- L8 persi: `AuthorizedL8LostCount`, blocchi delta seconds/hands min/avg/max, samples, timestamps
- Ultima auth L6: `LastL6AuthorizationAtUtc`, `PBHandsPlayed`, `LastL6AuthorizationPBHandsPlayed`, score/streak/shoeHand/avgHandSeconds
- Auth→L8 loss: 12 campi `AuthorizedL8LostFromAuthorization*` / `LastAuthorizedL8Loss*`

---

## 2. Campi necessari nel payload live

Analisi da `StatsWidget.vue` e `DashboardService`.

### Tier A — Persistiti in `TELEMETRY` (poll ogni ~5s, tutti i bot)

**Global Statistics + Spot + Pause** (~650 char, indipendenti da N bot):

`TotalPBHandsPlayed`, `TotalAuthL6Authorized`, `TotalL5Played/Won/Lost`, `TotalL8Played/Won/Lost`,  
`SpotID`, `SpotPBHandsPlayed`, `SpotAuthL6Counter`, `SpotL5Loss`,  
`GlobalPauseScalping`, `GlobalPauseScalpingDetails`, `GlobalPauseScalpingDuration`,  
`INC`, `EWMA`, `TotalPauseScalpingSoglieActivated`, `TotalPauseScalpingEWMAActivated`,  
`TotalSecurityFilterActivated`, `TotalSecurityFilterPreventedL6`, `LastAvgHandSeconds`, `ActiveSecurityFilterBots`,  
`BotMargins` (solo bot attivi)

**Control Room — card compatta per bot** (~350–400 char/bot, 18 campi):

| Campo | Uso UI |
|-------|--------|
| `AvgHandSeconds` | Card + score ritmo |
| `LastHandDeltaSeconds` | Trigger attenzione |
| `LastTwoHandDeltaSeconds` | Trigger rapido L5 |
| `RapidL5TriggerActive` | Badge rischio |
| `CurrentStreak` | Score + card |
| `SecurityRiskScore` | Score 0–4, colori |
| `SecurityFilterActive` | Pausa / stato |
| `PauseBot`, `PauseScope`, `PauseComputer` | Pausa scope |
| `PreventedL6` | Sintesi |
| `LastShoeHand` | Shoe vs soglia |
| `Martingala` | Livello L |
| `HasL6Credit` | (opzionale, non in card) |
| `LastReason` | Debug strip |
| `L6PlayedCount` | Solo contatore in lista (opzionale) |
| `AuthorizedL8LostCount` | Solo contatore in lista (opzionale) |

**Non serializzare a root** (già in DB `Configuration`):

`SecurityFilterEnabled`, `SecurityFilterMinScore`, `SecurityFilterMinStreak`,  
`SecurityFilterMaxShoeHand`, `SecurityFilterMaxAvgSeconds`, `SecurityFilterVeryFastSeconds`, `SecurityFilterDeltaWindow`

→ La UI li legge già da config con fallback; risparmio ~230 char/tick.

**Stima Tier A con slim summary:**

| Bot | Char totali | Entro 4000? |
|-----|-------------|-------------|
| 1 | ~1100 | sì |
| 2 | ~1450 | sì |
| 4 | ~2150 | sì |
| 8 | ~3550 | sì |
| 12 | ~4950 | limite |

### Tier B — On-demand (solo bot selezionato, pannello “Dettaglio aperto”)

Caricati **al click** su una card Control Room (~45 campi, ~1800 char):

- Ritmo completo: `MinHandDeltaSeconds`, `MaxHandDeltaSeconds`, `PBHandsPlayed`
- L6 timing: tutti `*L6Delta*`, `LastL6Played*`, `LastL6Authorization*`
- L8 auth perso: tutti `AuthorizedL8*`, `LastAuthorizedL8*`, `*FromAuthorization*`
- Aggregati missione bot: `Activations`, `HandSamples`, `LastUpdatedUtc`, `Avg*`, `Min*`, `Max*` non usati in card

### Tier C — Mai in JSON dashboard

- `Computer` duplicato (chiave dizionario basta)
- DateTime default `0001-01-01` (omit if default)
- Config SF duplicata a root

---

## 3. Soluzione strutturale (senza solo `NVARCHAR(MAX)`)

### Architettura target

```
Decisore (memoria piena)
    │
    ├─► AggiornaStatistiche  →  TELEMETRY = TelemetryPersistence (slim, Tier A)
    │
    └─► GET /api/proactive/security-filter/{computer}  →  SecurityFilterBotTelemetry (Tier B, da _securityFilterByBot)

WebApi
    │
    ├─► GET /api/Dashboard/telemetry  →  slim da DB + campi parsed (Tier A)
    │
    └─► GET /api/Dashboard/security-filter/{computer}  →  proxy Decisore (Tier B)

Frontend StatsWidget
    │
    ├─► Poll telemetry slim  →  Global Statistics + Control Room cards
    │
    └─► On select bot  →  fetch security-filter detail  →  pannello espanso
```

### Fasi implementazione

#### Fase 1 — Fix senza modifica DB (priorità)

1. **Decisore:** classe `TelemetryPersistence` + `ToPersistenceJson(Telemetry full)`
   - Serializza solo Tier A
   - Filtra `SecurityFilterByBot` → `SecurityFilterBotSummary` (18 campi)
   - Solo bot presenti in `BotMargins` / `_lastBotMargin`
2. **Decisore:** `GET /api/proactive/security-filter/{computer}` (read-only, da engine in-memory)
3. **WebApi:** `GET /api/Dashboard/security-filter/{computer}` proxy via `DeciderOptions`
4. **Frontend:** `StatsWidget` — al click bot, `loadBotDetail(computer)` merge in `selectedSecurityFilterRow`
5. **Frontend (resilience):** Global Statistics da campi API parsed (`totalPbHandsPlayed`, …) **non** solo `JSON.parse(rawTelemetry)`
6. **Log** (già preparato): `TELEMETRY_SIZE telemetryJson.Length=… numeroBot=… dimensioneSecurityFilterByBot=…`

#### Fase 2 — Hardening opzionale DB

- `ALTER COLUMN TELEMETRY NVARCHAR(MAX)` come **rete di sicurezza**, non fix primario
- Valutare solo se si vuole conservare blob completo per audit offline

#### Fase 3 — Pulizia

- Rimuovere duplicati config SF dal JSON persistito
- Prune bot stale da `_securityFilterByBot` quando assenti da `_lastBotMargin` > N minuti

---

## 4. Perché endpoint dedicati vs solo colonna più larga

| Approccio | Pro | Contro |
|-----------|-----|--------|
| Solo `NVARCHAR(MAX)` | Veloce | Payload 10k+ con 4–8 bot; parse JSON pesante ogni 5s; problema strutturale resta |
| **Slim + on-demand** | Entro 4000; poll leggero; dettaglio solo quando serve; separazione live/audit | 2 endpoint + piccolo refactor UI |
| Tabella separata `TelemetryDetail` | Storico audit | Over-engineering per caso attuale |

**Raccomandazione:** Fase 1 (slim + on-demand). `NVARCHAR(MAX)` opzionale come paracadute.

---

## 5. Misure verificate (riferimento)

| Scenario | Prima (`454becd`) | Dopo (`1324ade`+) | Slim proposto |
|----------|-------------------|-------------------|---------------|
| 1 bot | ~1001 | ~3031 prod 3091 | ~1100 |
| 2 bot | ~1402 | ~5232 prod 4000† | ~1450 |
| 4 bot | ~2204 | ~9634 | ~2150 |

† prod troncato SQL

Script riproducibili: `tools/telemetry-commit-size-audit.py`, `tools/telemetry-size-audit.py`

---

## 6. Checklist implementazione

- [ ] `TelemetryPersistence.cs` + mapping summary in `ProactiveEngine.getTelemetry()`
- [ ] `EngineController` → persist slim, log dimensioni
- [ ] `GET /api/proactive/security-filter/{computer}`
- [ ] `GET /api/Dashboard/security-filter/{computer}` + `DeciderController` proxy
- [ ] `StatsWidget.vue` — fetch detail on select
- [ ] `StatsWidget.vue` / `Dashboard.vue` — fallback campi API per Global Statistics
- [ ] Test: 2 bot → JSON valido ≤4000, Control Room visibile, dettaglio al click
- [ ] Deploy Decisore → WebApi → Firebase (ordine obbligatorio)

---

## Riferimenti codice

| Componente | Path |
|------------|------|
| Modello pieno | `decision-engine/Decisore/Engine/Telemetry.cs` |
| Writer | `decision-engine/Decisore/Controllers/EngineController.cs` → `UpdateMargin` |
| Reader | `backend/WebApi/Services/Implementations/DashboardService.cs` |
| UI | `frontend/src/components/dashboard/StatsWidget.vue` |
| DDL colonna | `ops/dash2a-readiness/production-30-tables-ddl.sql` |
