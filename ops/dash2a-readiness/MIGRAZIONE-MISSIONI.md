# Migrazione controllata missioni: locale → produzione DASH2A

> **Strategia attiva (v2):** import **missione-per-missione** con INSERT only, date/chiavi rigenerate, rollback SQL per ogni missione.  
> **Deprecato:** backup globale + bulk import (`import-missions-to-prod.ps1` / `mission-migration-apply-prod.yml`).

> **Regola base:** vietato restore completo del DB locale sopra produzione.  
> Si migrano **solo** `MissionSessions` e `MissionMarginSamples` con insert idempotenti.

---

## Strategia v2 — One-by-one (INSERT only)

Script: `ops/dash2a-readiness/import-missions-one-by-one.ps1`

| Parametro | Valori | Default |
|---|---|---|
| `-Mode` | `DryRun`, `Apply` | `DryRun` |
| `-Source` | `Export`, `LocalDb` | `Export` |
| `-RuntimeModeFilter` | `Production`, `All` | `Production` |
| `-RegenerateDates` | switch | on (nuove `MissionKey`, `StartTime`, `EndTime`) |
| `-OneByOne` | switch | on (stop al primo errore in Apply) |
| `-MetaFile` | path meta export | auto da `exports/` |
| `-SequenceStartDate` | datetime | `2025-01-01T08:00:00Z` |
| `-SequenceGapMinutes` | int | `1440` (1 giorno tra missioni) |

Per ogni missione locale:

1. INSERT nuova `MissionSession` (`Completed=true`, `RuntimeMode=Production`, `FinalizationReason=OneByOneHistoricalImport`)
2. Nuovo ID da identity SQL Server (no `IDENTITY_INSERT`)
3. Bulk INSERT campioni `MissionMarginSamples` con timestamp shiftati (durata preservata)
4. Verifica conteggi (+1 sessione, +N campioni)
5. File rollback: `DELETE` campioni + sessione **solo** sul nuovo ID

**Vietato:** UPDATE/DELETE/TRUNCATE su righe prod esistenti (sessioni live 1–8).

### Dry-run locale (PC dev)

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
powershell -ExecutionPolicy Bypass -File .\ops\dash2a-readiness\import-missions-one-by-one.ps1 `
  -Source Export `
  -MetaFile .\ops\dash2a-readiness\exports\missions_export_meta_20260530_011346.json `
  -RuntimeModeFilter Production `
  -Mode DryRun
```

Output: `ops/dash2a-readiness/exports/one-by-one/onebyone_*/` con `run_summary.json`, `candidates.json`, `rollback/*.sql`.

### Dry-run / Apply VPS (workflow)

```powershell
gh workflow run "DASH2A Mission Migration One-By-One" `
  --repo eugeniorossi2025-sudo/TradingDashboard-2a `
  -f confirm=MISSION_ONE_BY_ONE_DRY_RUN `
  -f mode=DryRun `
  -f runtime_mode_filter=Production `
  -f release_tag=mission-migration-staging-20260530 `
  -f regenerate_dates=true
```

Apply reale (solo dopo autorizzazione esplicita):

```powershell
gh workflow run "DASH2A Mission Migration One-By-One" `
  --repo eugeniorossi2025-sudo/TradingDashboard-2a `
  -f confirm=MISSION_ONE_BY_ONE_APPLY `
  -f mode=Apply `
  -f runtime_mode_filter=Production `
  -f release_tag=mission-migration-staging-20260530 `
  -f regenerate_dates=true
```

Preflight Apply: produzione ferma, zero missioni con `Completed=0`.

---


## Contesto (da `DASH2A-INFRASTRUCTURE.md`)

| Ambiente | DB | Tabelle missione |
|---|---|---|
| **Locale** | `(localdb)\MSSQLLocalDB` / `Dash2A_LocalProdLike` | ~67 sessioni, ~227k campioni |
| **Produzione** | `51.83.159.175,1434` / `Eugenio-Demo10` | ~7 sessioni live (mag 2026) |

**Produzione attuale (API, mag 2026):** sessioni ID 1–7 generate dal motore live — **non toccare, non sovrascrivere**.

**Locale:** 67 sessioni uniche per `MissionKey` (48 Demo + 19 Production), import storico `historical-demo-import:*`.

---

## Principi di sicurezza

1. **NO** `RESTORE DATABASE` produzione da backup locale  
2. **NO** `TRUNCATE` / `DELETE` / `UPDATE` su righe prod esistenti  
3. **NO** `IDENTITY_INSERT` — nuovi ID prod assegnati automaticamente  
4. **NO** EF migrations in produzione  
5. **SÌ** backup prod verificato prima dell'import reale  
6. **SÌ** dry-run obbligatorio prima dell'apply  
7. **SÌ** skip automatico se `MissionKey` già presente in prod (indice unique)  
8. **SÌ** transazione per sessione (session + campioni figli)  
9. **SÌ** report JSON post-operazione  

**Fuori scope:** `Users_v2`, `Configurations`, `Pc_CurrentStatus`, `Margini`, `Statistiche`, `ApiLogs` — restano invariati.

---

## Fase 0 — Preflight produzione (read-only)

```powershell
gh workflow run "DASH2A Mission DB Readonly Diagnostic" `
  --repo eugeniorossi2025-sudo/TradingDashboard-2a `
  -f confirm=MISSION_DB_READONLY
```

Verificare: conteggio sessioni, assenza duplicati `MissionKey`, schema tabelle OK.

---

## Fase 1 — Export dal DB locale (PC dev)

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"

# Tutte le 67 missioni
powershell -ExecutionPolicy Bypass -File .\ops\dash2a-readiness\export-missions-local.ps1 -RuntimeMode All -Compress

# Solo Production (19 sessioni)
powershell -ExecutionPolicy Bypass -File .\ops\dash2a-readiness\export-missions-local.ps1 -RuntimeMode Production -Compress
```

Output in `ops/dash2a-readiness/exports/` (gitignored):

- `missions_sessions_YYYYMMDD_HHMMSS.json`
- `missions_samples_YYYYMMDD_HHMMSS.jsonl`
- `missions_export_meta_YYYYMMDD_HHMMSS.json`
- opzionale: `missions_export_YYYYMMDD_HHMMSS.zip`

---

## Fase 2 — Trasferimento export sul VPS backend

Copiare lo zip/cartella export su:

```text
C:\inetpub\wwwroot\backups\mission-migration\
```

(es. via RDP, SCP, o upload manuale — **non** committare export in git)

---

## Fase 3 — Dry-run import su produzione (VPS / runner backend)

```powershell
cd C:\inetpub\wwwroot\backups\mission-migration
Expand-Archive -Path .\missions_export_YYYYMMDD_HHMMSS.zip -DestinationPath .\staging -Force

powershell -ExecutionPolicy Bypass -File C:\path\to\repo\ops\dash2a-readiness\import-missions-to-prod.ps1 `
  -MetaFile .\staging\missions_export_meta_YYYYMMDD_HHMMSS.json `
  -DryRun
```

Controllare nel report:

- quante sessioni **would_insert** vs **skipped** (MissionKey già in prod)
- campioni attesi per sessione
- zero errori

---

## Fase 4 — Backup produzione (obbligatorio)

Sul VPS backend, **prima** dell'import reale:

```sql
BACKUP DATABASE [Eugenio-Demo10]
TO DISK = N'C:\inetpub\wwwroot\backups\Eugenio-Demo10_pre_mission_import_YYYYMMDD.bak'
WITH INIT, COMPRESSION, STATS = 5;
```

---

## Fase 5 — Import reale

```powershell
powershell -ExecutionPolicy Bypass -File .\ops\dash2a-readiness\import-missions-to-prod.ps1 `
  -MetaFile .\staging\missions_export_meta_YYYYMMDD_HHMMSS.json `
  -SkipBackupRecommendation
```

Oppure via workflow GitHub (vedi sotto).

---

## Fase 6 — Verifica post-import

1. Rieseguire diagnostic read-only (Fase 0)  
2. API autenticata:

```text
GET /api/mission/reports/index?runtimeMode=All&limit=200
```

3. UI Firebase → report missioni / financial report  
4. Confermare: sessioni prod 1–7 **intatte**, nuove sessioni aggiunte con ID > max esistente  

---

## Workflow GitHub (opzionale)

```powershell
gh workflow run "DASH2A Mission Migration Controlled" `
  --repo eugeniorossi2025-sudo/TradingDashboard-2a `
  -f confirm=MISSION_MIGRATION_DRY_RUN `
  -f dry_run=true `
  -f export_meta_path=C:\inetpub\wwwroot\backups\mission-migration\staging\missions_export_meta_....json
```

Per apply reale: `confirm=MISSION_MIGRATION_APPLY`, `dry_run=false` — **solo dopo backup e dry-run OK**.

---

## Scelta RuntimeMode

| Opzione | Sessioni | Quando usarla |
|---|---:|---|
| `All` | 67 | Storico completo in dashboard prod |
| `Production` | 19 | Solo missioni etichettate Production |
| `Demo` | 48 | Solo dati demo/import storico |

**Raccomandazione:** partire con dry-run `All`; se in prod si vogliono solo dati reali, usare `Production`.

---

## Rischi e mitigazioni

| Rischio | Mitigazione |
|---|---|
| Collisione MissionKey | Skip automatico + indice unique |
| Sovrascrittura sessioni live 1–7 | Solo INSERT, mai UPDATE; keys diverse (`pbt-*` vs `historical-demo-import:*`) |
| Volume campioni (~227k) | SqlBulkCopy a batch da 5000 |
| Missione live in corso | Eseguire solo a missione ferma (`MISSIONE_FERMA_CONFERMATA`) |
| Rollback | Restore backup Fase 4 (solo tabelle missione se necessario DELETE selettivo dal report) |

---

## Rollback selettivo (estremo)

Se import errato **solo** su sessioni nuove (ID > 7):

```sql
-- SOLO dopo aver identificato gli ID inseriti dal report import_report_*.json
BEGIN TRAN;
DELETE FROM dbo.MissionMarginSamples WHERE SessionId IN (...);
DELETE FROM dbo.MissionSessions WHERE ID IN (...);
COMMIT;
```

**Mai** cancellare ID 1–7 senza review esplicita.
