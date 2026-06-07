# AGENT-GIACOMO1 — GameBot Giacomo DASH2A (documento unico)

> **Agente:** `giacomo1` / `/giacomo1`
> **Scope:** patch/build/deploy GameBot Giacomo — **NON** dashboard Vue/WebApi (`giacom`), **NON** Eugenio EUGENIO13 (`marco1`)
> **Marker:** `agent-giacomo1:v2026-06-06`
> **Leggere SOLO questo file** per task bot Giacomo.

---

## Identità e separazione obbligatoria

| | **giacomo1 (questo agente)** | **giacom (altro agente)** | **marco1 (Eugenio)** |
|--|-------------------------------|--------------------------|----------------------|
| Sorgente | `tools/eugenio-bot` | frontend, WebApi, Decisore | `baccarat-bot-main-socket-artifact` |
| Runtime attivo doc | `relisegiacomo ok 1.6` | — | `EUGENIO13` |
| Linea target agente | `relisegiacomo ok 1.7` (cartella **non** presente al 2026-06-06) | — | OCR v5f |

**Clone obbligatorio:** `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`
**Remote:** `eugeniorossi2025-sudo/TradingDashboard-2a`

**VIETATO:** `baccarat-bot-main-socket-artifact`, `EUGENIO13`, `tools/eugenio-gamebot/source`, cartelle Desktop `Gamebot_FirstPlay_*`, repo Dashboard 1.

---

## PARTE A — Workspace Guard

# DASH2A — Workspace Guard (obbligatorio)

> **Leggere questo file prima di ogni audit, patch, commit, validazione pre-deploy o attività agent su DASH2.**
> Se una condizione non è soddisfatta: **fermarsi subito** e non modificare nulla.

---

## 1. Repository corretto

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

URL GitHub:

```text
https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git
```

---

## 2. Clone locale corretto

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

---

## 3. Branch operativo

```text
main
```

---

## 4. Regola assoluta

**Qualsiasi attività su DASH2 deve partire da questo path:**

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

Non aprire, non patchare e non committare da altri workspace Cursor.

---

## 5. Checklist obbligatoria (prima di ogni audit / patch / commit)

Eseguire **sempre** dalla root del clone DASH2A:

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
git remote -v
git branch --show-current
git status
git rev-parse --show-toplevel
```

Verificare che l'output sia coerente con le sezioni 1–3 di questo file.

---

## 6. Stop immediato se il path non è quello corretto

Se `git rev-parse --show-toplevel` **non** restituisce esattamente:

```text
C:/Users/eugen/Desktop/NuovaDashboard-MarcoTurri
```

(o l'equivalente con backslash `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`)

→ **fermarsi subito. Non modificare nulla.**

---

## 7. Stop immediato se il remote non è quello corretto

Se `git remote -v` **non** contiene:

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

→ **fermarsi subito. Non modificare nulla.**

Remote tipici **vietati** per lavori DASH2:

- `TradingDashboard-iis`
- repo / path legati a Dashboard 1
- `PCTEST45\TradingDashboard`
- qualsiasi clone diverso da `NuovaDashboard-MarcoTurri`

---

## 8. Dashboard 1 — fuori scope

Dashboard 1 (**TradingDashboard-iis**, `PCTEST45\TradingDashboard`, Eugenio-Demo10, Firebase Dashboard 1, VPS legacy IIS) è **fuori scope** per DASH2.

| Azione | Consentita per DASH2? |
|--------|----------------------|
| Leggere codice Dashboard 1 | **No** |
| Patchare Dashboard 1 | **No** |
| Committare su Dashboard 1 per lavori DASH2 | **No** |
| Usare credenziali / DB / URL Dashboard 1 | **No** |

Per DASH2 usare **solo** il clone e il remote indicati in questo documento.

---

## Riferimenti correlati

- Infrastruttura completa: [`DASH2A-INFRASTRUCTURE.md`](../../DASH2A-INFRASTRUCTURE.md) (root repo)
- Report contabilità: [`REPORT-CONTABILITA-CANONICA.md`](./REPORT-CONTABILITA-CANONICA.md)
- Validazione report: [`validate-report-coherence.ps1`](./validate-report-coherence.ps1)

---

*Documento operativo DASH2A — aggiornato 2026-05-30*


---

## PARTE B — GameBot §14 (contenuto canonico)
## 14. GAMEBOT — SORGENTE E RELEASE (documento padre)

> **Regola assoluta:** patch, build e deploy GameBot DASH2A **solo** su `tools/eugenio-bot`.  
> **NON** usare: `tools/eugenio-gamebot/source`, `Documents\baccarat-bot-main OK`, `baccarat-bot-main-socket-artifact`, cartelle Desktop `Gamebot_FirstPlay_*`, repo Dashboard 1 / IIS.

### 14.1 Sorgente canonico (1.5 — base 1.4 / 1.3 / 1.2 unified)

| Voce | Percorso |
|------|----------|
| **Sorgente unico** | `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri\tools\eugenio-bot` |
| **Solution** | `tools\eugenio-bot\Gamebot.sln` |
| **Output build** | `tools\eugenio-bot\Gamebot\bin\Release\` |
| **Build** | `Release` \| `Any CPU` |

```text
MSBuild tools\eugenio-bot\Gamebot.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild
```

### 14.2 Matrice feature per release

| Pezzo | 1.0 | 1.1 | 1.2 | 1.3 | 1.4 | 1.5 | **1.6** (runner attivo) |
|-------|-----|-----|-----|-----|-----|-----|-------------------------|
| Probe Banker FirstPlay | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Probe nuovo mazzo (mazzo 0) | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ (solo >30) |
| AC2 → PAUSE_SCALPING | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BotOwner gate (Step 3) | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Skip SCULPING se già oltre `limitEndDeck` | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| NEW_DECK / PAUSE da soglia (`LIMIT_MIN_NEW_DECK`) | ❌ | ❌ | ❌ | mano **1–30** | mano **15–30** (bug) | mano **14–30** | ✅ mano **9–30** |
| WAITING probe (`StateAttendiNuovoMazzo`) | ❌ | ❌ | ✅ (0, >30) | ✅ (0, >30) | ❌ (solo log) | ✅ ogni mano 0–13 | ✅ **solo 3/6/9** (+ >30) |
| Preset `pause_sculping_counter=1` ingresso PAUSE da waiting | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ (bet canonica ~mano 11) |

**File patch 1.2 unified (9) — ereditati da 1.5:**

- `Gamebot\Models\SubStates\StateFirstPlay.cs`
- `Gamebot\Models\SubStates\StateAttendiNuovoMazzo.cs`
- `Gamebot\Helpers\DashboardApiHelper.cs`
- `Gamebot\Models\SubStates\StateSculping.cs`, `StateSafeWin.cs`, `StateFineMazzo.cs`, `StatePauseSculping.cs` — `RequestExit()`
- `Gamebot\Helpers\BotOwnerAuthHelper.cs` + gate in `Configuratore.cs` (già presenti)

**Origine patch (solo riferimento storico, non lavorare lì):**

| Patch | Fonte storica |
|-------|----------------|
| Probe FirstPlay / nuovo mazzo | `Documents\baccarat-bot-main OK\baccarat-bot-main` |
| AC2 Post-AC2 | `baccarat-bot-main-socket-artifact` (2026-06-03) |
| BotOwner | commit STEP 3 su `tools/eugenio-bot` |
| Skip SCULPING oltre limite (1.3) | `MainStateBot.cs` — promosso 2026-06-06 |
| NEW_DECK min mano 15 (1.4) | `Constants.cs` + `MainStateBot.cs` — promosso 2026-06-06 (sostituito da 1.5) |

**File patch 1.3 (1):** `Gamebot\Models\MainState\MainStateBot.cs` — `IsPastEndDeckLimit()` + guard in `FIRST_PLAY`.

**File patch 1.4 (2):** `Constants.cs` — `LIMIT_MIN_NEW_DECK = 15`; `MainStateBot.cs` — attesa mani 1–14 in `WAITING_NEW_DECK` (regressione: solo log).

**File patch 1.5 (2):** `Constants.cs` — `LIMIT_MIN_NEW_DECK = 14`; `MainStateBot.cs` — ripristino ramo `else` 1.3 (`StateAttendiNuovoMazzo.Act()` su mani 0–13 e >30).

**File patch 1.6 (3):** `Constants.cs` — `LIMIT_MIN_NEW_DECK = 9`; `StateAttendiNuovoMazzo.cs` — probe solo mani **3/6/9**, skip altre mani; `MainStateBot.cs` — uscita waiting dopo probe mano **9**, preset `pause_sculping_counter = 1` → PAUSE.

### 14.3 Runtime CRIPTOOK — hash e uso

| Runtime | Uso | Hash `Gamebot.exe` | Note |
|---------|-----|-------------------|------|
| `relisegiacomo ok` | Stabile legacy | `DFEF9CDC…` | Non promuovere |
| **`relisegiacomo ok 1.0`** | **Baseline congelata** | `D1A8AAF0…` | AC2 sì, probe no — **non sovrascrivere** |
| `relisegiacomo ok 1.1` | Release BotOwner | `7C6399C5…` | BotOwner sì, probe/AC2 no |
| `relisegiacomo ok 1.2` | Unified precedente | `C86DAD1A…` | Congelato |
| `relisegiacomo ok 1.3` | Release precedente | `A256CC22…` | Congelato |
| `relisegiacomo ok 1.4` | Release precedente | `BAA75A8D…` | Congelato — sostituito da **1.5** |
| `relisegiacomo ok 1.5` | Release precedente | `59E7A6E0…` | Congelato — sostituito da **1.6** |
| **`relisegiacomo ok 1.6`** | **Release attiva / runner Giacomo** | `1BA571C2…` | waiting probe 3/6/9, uscita mano 9, preset PAUSE counter — promosso 2026-06-06 |

**Path assoluto runtime release 1.6 (runner attivo):**

```text
C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.6\Gamebot.exe
SHA256: 1BA571C214745446CA8249AC0EB5A203B1CDE28F4916BC3F24A31226E1B8DA5D
```

**ZIP release 1.6 (intera cartella runtime — distribuire questo):**

```text
C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.6.zip
SHA256: F3D9D64BC5AF1DDC6B283F57C4A3ED773CA84C0F89D95855CC15244A3E504C76
```

**Path runtime 1.5 (congelato):**

```text
C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.5\Gamebot.exe
SHA256: 59E7A6E0BAE16FD3B287E502C52395E987EF681558A75CB9B08DEA2954C3313E
```

**ZIP release 1.5 (congelato):**

```text
C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.5.zip
SHA256: D6CF13B22B4F9B7978FE49E3A7787835F6277036B91FFCF3F4F185870096A6BB
```

> **`relisegiacomo ok 1.4`** — congelato (non runner). **`1.3`** / **`1.2`** — congelati.

| `Gamebot_FirstPlay_ProbeUntilPlayer_*_20260320` | Riferimento storico probe | `02DC9D5A…` | Artefatto Desktop — non sorgente |

Hash completi (verifica pre-deploy):

```text
1.0  D1A8AAF0F6AAF50BAB41FFE19DDE2B19904D0723352CDDA1179D0E520150C273
1.1  7C6399C55BE04A34CAB499F3CF84E84624ABED6D31B189A486D0FEAA012BA697
1.2  C86DAD1A401C65A31E19FFB83A1B1F474C06883AE96448860CFD8C656B706E15
1.3  A256CC229898F2EF706DC5167506C793E6067B15759F128731D3F51BD9953915
1.4  BAA75A8D7FC33B26D2E3FA392E5053A1DFFD677F6F5104F28F09154C1AD020DA
1.5  59E7A6E0BAE16FD3B287E502C52395E987EF681558A75CB9B08DEA2954C3313E
1.6  1BA571C214745446CA8249AC0EB5A203B1CDE28F4916BC3F24A31226E1B8DA5D
```

**Deploy runtime:** copiare solo `Gamebot.exe` + `Gamebot.pdb` nella cartella test/prod scelta. Non sostituire `Gamebot.exe.config` se non cambia intenzionalmente.

### 14.4 Blindare il sorgente (obbligatorio)

1. **Commit + tag** su `TradingDashboard-2a` dopo ogni modifica GameBot:
   ```text
   git tag gamebot-1.6-waiting-probe-369-pause-preset-2026-06-06
   ```
2. **Mai** patch su exe runtime o cartelle Desktop senza commit su `tools/eugenio-bot`.
3. **Pre-build check** (stringhe attese in `bin\Release\Gamebot.exe`):
   - `FaiProbeRossaMinima`
   - `FIRST_PLAY | PROBE ROSSA MINIMA`
   - `WAITING NEW DECK | PROBE ROSSA MINIMA`
   - `WAITING NEW DECK | SKIP PROBE` (1.6+)
   - `RequestExit`
   - `SALTO SCULPING` (1.3+)
   - **Non** deve comparire `ATTESA MANO 15+` (regressione 1.4 rimossa)
4. **Pre-promozione runtime:** hash build = hash cartella destinazione; `1.0` hash invariato se non promozione esplicita.
5. **Cursor / sessione:** citare sempre §14 di questo file; non cercare sorgente in chat vecchie o path alternativi.
6. **Release runtime attiva:** `relisegiacomo ok 1.6` — runner Giacomo; non usare cartelle `-test`; mai sovrascrivere `1.0` senza decisione esplicita.

### 14.5 Log attesi in release 1.6

- Tutti i log **1.5** ereditati (probe FirstPlay, AC2, BotOwner, skip SCULPING oltre limite)
- `WAITING NEW DECK | SKIP PROBE | NUMBER_DECK: …` — mani waiting **senza** probe (1,2,4,5,7,8)
- `WAITING NEW DECK | PROBE ROSSA MINIMA | NUMBER_DECK: 3|6|9` — sole probe waiting
- `WAITING_NEW_DECK | MAZZO NUOVO (9)` — transizione a `PAUSE_SCALPING` o `NEW_DECK`
- `PAUSE_SCULPING | PRESET CONTATORE: 1` — ingresso PAUSE da waiting
- `GIOCATA RANDOM Pause Sculping | Runtime PauseScalping: 3` — prima bet canonica PAUSE (~mano **11**)

---
---

## Nota linea 1.7 (agente giacomo1)

- L'agente **giacomo1** lavora sulla **linea evolutiva 1.7** in `tools/eugenio-bot`.
- **Runtime documentato attivo al 2026-06-06:** `relisegiacomo ok 1.6` (hash in tabella sopra).
- **Runtime target 1.7:** `C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.7\` — **cartella assente** fino a promozione esplicita; non inventare hash 1.7.
- Promozione 1.6 → 1.7: commit + tag su `TradingDashboard-2a`, build Release, verifica stringhe §14.4, hash build = hash runtime destinazione.

---

## Checklist agente giacomo1 (obbligatoria)

- [ ] `git rev-parse --show-toplevel` = `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`
- [ ] `git remote -v` contiene `TradingDashboard-2a`
- [ ] Patch **solo** in `tools/eugenio-bot` (non exe Desktop senza commit)
- [ ] **Non** confondere con Eugenio: OCR v5f / waiting mani 1–30 = **marco1**, non Giacomo
- [ ] Pre-promozione: hash build = hash cartella runtime; tag git dopo ogni release
- [ ] **Non** letto `AGENT-GIACOM.md` salvo chiarimento infra condivisa
