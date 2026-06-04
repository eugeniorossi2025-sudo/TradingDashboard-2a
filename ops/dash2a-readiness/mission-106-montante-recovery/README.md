# Mission #106 — Montante recovery post-#105 (2026-06-04)

**Bozza validata:** `artifacts/log-audit-readonly/RECOVERY-CONTABILE-FINALE-POST105.md`  
**TotalMargin operativo:** **+340,60 €** (lordo progressioni vincenti chiuse)  
**#105:** **non modificare** (+254,40 €)

## Convenzione orari SQL (`MissionSessions`)

Come missione #105 su PROD: colonne `StartTime` / `EndTime` = **UTC** (wall-clock Rome − 2h estate).

| Campo | Europe/Rome | UTC (DB) |
|-------|-------------|----------|
| Start | 2026-06-04 **16:25:10** | `2026-06-04T14:25:10` |
| End | 2026-06-04 **20:01:20** | `2026-06-04T18:01:20` |

## Step 1 — Dry-run (READ ONLY)

```powershell
# Da repo root, su runner DASH2A o con sqlcmd verso 51.83.159.175,1434
sqlcmd -S "51.83.159.175,1434" -d Eugenio-Demo10 -U sa3 -P "<password>" `
  -i ops/dash2a-readiness/mission-106-montante-recovery/01-dry-run-mission-106-montante-recovery.sql
```

Oppure GitHub Actions: workflow **DASH2A Mission 106 Montante Recovery Dry-Run** con conferma `MISSION_106_DRY_RUN`.

## Step 2 — INSERT (solo dopo approvazione esplicita)

```powershell
sqlcmd -v Confirm="INSERT_MISSION_106_MONTANTE" `
  -i ops/dash2a-readiness/mission-106-montante-recovery/02-insert-mission-106-montante-recovery.sql
```

Poi: `03-verify-mission-106-montante-recovery.sql` (readonly).

**Non** eseguire INSERT senza conferma scritta. Nessun commit/deploy automatico da questi script.
