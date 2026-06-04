# Bonifica overlap temporale — MissionSessions #101–#104

Corregge **solo** `StartTime` / `EndTime` (in pratica solo `EndTime` di #101–#103) e `FinalizationReason` (#101–#103 → `OverlapTimeRecovery`).

**Non modifica:** `MissionMarginSamples`, `TotalMargin`, `RealHandsCount`, `MissionKey`, `Completed`, #104 `EndTime` / `FinalizationReason`.

## Regola applicata

| Sessione | StartTime | EndTime proposto |
|----------|-----------|----------------|
| #101 | invariato | = `StartTime` di #102 |
| #102 | invariato | = `StartTime` di #103 |
| #103 | invariato | = `StartTime` di #104 |
| #104 | invariato | invariato |

I valori esatti sono letti dal DB al momento dell’esecuzione (nessun hardcode).

## Ordine di esecuzione (produzione)

Eseguire su SQL Server DASH2A (stesso DB di `appsettings.Production.json`), ad es. con `sqlcmd`:

```powershell
$conn = '<DefaultConnection from shared config>'
sqlcmd -S ... -d ... -i ops\dash2a-readiness\mission-overlap-time-recovery\01-dry-run-overlap-time-recovery.sql
sqlcmd ... -i 02-backup-mission-sessions-101-104.sql
# Verificare output dry-run e nome tabella backup
sqlcmd ... -i 03-apply-overlap-time-recovery.sql
sqlcmd ... -i 04-verify-overlap-time-recovery.sql
```

Oppure workflow readonly/diag sul runner `DASH2A-BACKEND` copiando gli script.

## Rollback

```sql
-- Sostituire <BACKUP_TABLE> con il nome stampato da 02-backup (es. MissionSessions_bkp_overlap_time_20260604)
UPDATE m
SET
    m.StartTime = b.StartTime,
    m.EndTime = b.EndTime,
    m.FinalizationReason = b.FinalizationReason,
    m.ReportPublishedAt = b.ReportPublishedAt
FROM dbo.MissionSessions m
INNER JOIN dbo.<BACKUP_TABLE> b ON b.ID = m.ID
WHERE m.ID IN (101, 102, 103, 104);
```

## Verifica UI

Dopo `04-verify`, controllare la lista missioni nel report: finestre #101→#104 devono risultare consecutive senza overlap.
