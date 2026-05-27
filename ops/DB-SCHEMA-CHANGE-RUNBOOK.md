# DASH2A — Procedura Ufficiale Modifica Schema DB

> **Documento operativo vincolante.**
> Seguire ogni passo nell'ordine indicato. Nessun DDL in produzione senza aver completato la fase di verifica.
> **Ultimo aggiornamento: 2026-05-27 02:52 CEST** — target DB unico `51.83.159.175,1434`; runner backend e runner Decisore verificati.

---

## 1. DATABASE PRODUZIONE IN USO

| Istanza | Host | Porta | Usata da |
|---|---|---|---|
| SQLEXPRESS01 | `51.83.159.175` | **1434** | WebApi dashboard + Decisore Proattivo (`sa3`) |

> **Regola:** il target operativo produzione è `51.83.159.175,1434` / `Eugenio-Demo10`. Non usare `1433` per nuove modifiche DASH2A: è uno storico/deriva di vecchie configurazioni e non va considerato runtime senza nuova diagnostica esplicita.

### Runner e path operativi

| Ambito | Runner / path |
|---|---|
| Workflow DB e WebApi | `dash2a-backend-runner-01` (`DASH2A-BACKEND`) su `51.83.159.175`, machine `WIN-P8JPV1DNSB6` |
| Workflow Decisore | `dash2a-decisore-runner-01` (`DASH2A-DECISORE`) su `51.178.16.37`, machine `WIN-05FHTP223IE` |
| WebApi release root | `C:\inetpub\wwwroot\releases` |
| WebApi config live | `C:\inetpub\wwwroot\shared\appsettings.Production.json` |
| Decisore root | `C:\Decisore` |
| Decisore backup root | `C:\DecisoreBackups` |
| Decisore app pool | `Proactive` |

---

## 2. QUANDO USARE QUESTA PROCEDURA

Applicare obbligatoriamente per:

- Aggiunta di nuove colonne a tabelle esistenti
- Creazione di nuove tabelle
- Modifica / drop di colonne
- Creazione / modifica / drop di stored procedure
- Aggiunta di indici o constraint
- Qualsiasi ALTER TABLE in produzione

**Non serve** per: semplici INSERT/UPDATE/DELETE di dati (es. configurazioni).

---

## 3. FASI OBBLIGATORIE

### FASE 1 — Verifica stato attuale

Prima di qualsiasi modifica eseguire una query read-only per capire lo stato reale.

**Connessione da runner (workflow GitHub Actions):**

```powershell
# DB runtime produzione DASH2A — WebApi + Decisore
$cs = "Server=51.83.159.175,1434;Database=Eugenio-Demo10;User Id=sa3;Password=<SECRET>;Encrypt=False;TrustServerCertificate=True;Connect Timeout=10;"
```

**Query di verifica:**

```sql
-- Tutte le tabelle
SELECT name, create_date, modify_date FROM sys.objects WHERE type = 'U' ORDER BY name;

-- Colonne di una tabella specifica
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'NomeTabella'
ORDER BY ORDINAL_POSITION;

-- Tutte le stored procedure
SELECT name, create_date, modify_date FROM sys.objects WHERE type = 'P' ORDER BY name;

-- Esistenza oggetto specifico
SELECT OBJECT_ID('dbo.NomeOggetto', 'U');   -- tabella
SELECT OBJECT_ID('dbo.NomeSP', 'P');        -- stored procedure
```

---

### FASE 2 — Backup pre-modifica

**Per tabelle con dati critici**, eseguire un backup della struttura e dei dati tramite workflow dal runner:

```powershell
# Script backup tabella (eseguire via workflow runner)
$cs = "Server=51.83.159.175,1434;Database=Eugenio-Demo10;User Id=sa3;Password=<SECRET>;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
$conn.Open()

# Backup dati in tabella temporanea con timestamp
$ts = Get-Date -Format "yyyyMMdd_HHmm"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * INTO dbo.NomeTabella_bkp_$ts FROM dbo.NomeTabella"
$cmd.ExecuteNonQuery()
Write-Host "Backup creato: dbo.NomeTabella_bkp_$ts"
$conn.Close()
```

> Le tabelle di backup `_bkp_YYYYMMDD_HHmm` possono essere rimosse dopo 7 giorni di stabilità.

---

### FASE 3 — DDL script in repo

**Regola:** ogni modifica DDL deve essere scritta come file SQL nel repo prima di essere eseguita.

Percorso standard: `ops/db-migrations/YYYYMMDD_descrizione.sql`

**Formato file:**

```sql
-- =============================================================
-- DASH2A DB Migration
-- Data: 2026-MM-DD
-- Autore: <nome>
-- Istanza target: SQLEXPRESS01 (1434) runtime produzione
-- Descrizione: <cosa cambia e perché>
-- =============================================================

-- PRE-CHECK: verificare che l'oggetto non esista già
IF OBJECT_ID('dbo.NuovaTabella', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NuovaTabella (
        ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        -- ... colonne
    );
    PRINT 'dbo.NuovaTabella CREATA';
END
ELSE
BEGIN
    PRINT 'dbo.NuovaTabella GIA ESISTENTE - skip';
END

-- POST-CHECK: confermare la creazione
SELECT OBJECT_ID('dbo.NuovaTabella', 'U') AS ObjectId;
```

> Usare sempre `IF NOT EXISTS` / `IF OBJECT_ID IS NULL` per rendere gli script **idempotenti** (rieseguibili senza errori).

---

### FASE 4 — Esecuzione via workflow GitHub Actions

Non eseguire mai DDL direttamente da SSMS o da macchina locale. Usare sempre il runner self-hosted sul VPS backend.

**Template workflow (creare file `.github/workflows/db-migration-YYYYMMDD.yml`):**

```yaml
name: DB Migration - YYYYMMDD descrizione

on:
  workflow_dispatch:
    inputs:
      confirm:
        description: "Type EXECUTE_MIGRATION"
        required: true
        type: string

jobs:
  migrate:
    if: ${{ github.event.inputs.confirm == 'EXECUTE_MIGRATION' }}
    runs-on:
      - self-hosted
      - Windows
      - DASH2A
      - DASH2A-BACKEND
    timeout-minutes: 10

    steps:
      - uses: actions/checkout@v4

      - name: Execute migration
        shell: powershell
        env:
          DB_PASSWORD: ${{ secrets.DECISORE_DB_PASSWORD }}
        run: |
          $cs = "Server=51.83.159.175,1434;Database=Eugenio-Demo10;User Id=sa3;Password=$env:DB_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
          $sql = Get-Content "ops/db-migrations/YYYYMMDD_descrizione.sql" -Raw
          $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
          $conn.Open()
          $cmd = $conn.CreateCommand()
          $cmd.CommandText = $sql
          $cmd.ExecuteNonQuery()
          Write-Host "Migration eseguita OK"
          $conn.Close()
```

---

### FASE 5 — Verifica post-migrazione

Dopo l'esecuzione del workflow verificare:

```powershell
# Connessione diretta (da runner o script locale se porta aperta)
# Verificare esistenza tabella/colonna/SP
SELECT OBJECT_ID('dbo.NuovaTabella', 'U');

# Verificare struttura colonne
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'NuovaTabella';

# Test insert/select minimo
INSERT INTO dbo.NuovaTabella (...) VALUES (...);
SELECT TOP 1 * FROM dbo.NuovaTabella;
```

---

### FASE 6 — Test applicazione

Dopo ogni migrazione DB:

1. **Decisore** — riavviare app pool `Proactive` sul VPS `51.178.16.37`:
   ```powershell
   C:\Windows\System32\inetsrv\appcmd stop apppool /apppool.name:"Proactive"
   C:\Windows\System32\inetsrv\appcmd start apppool /apppool.name:"Proactive"
   Start-Sleep 6
   Test-NetConnection 127.0.0.1 -Port 80
   ```
   Non usare `/api/proactive/reset` come healthcheck neutro: svuota `Pc_CurrentStatus`.

2. **WebApi** — smoke test:
   ```powershell
   Invoke-WebRequest http://51.83.159.175/api/Auth/test -UseBasicParsing
   ```

3. Attesa minima **2 minuti** prima di dichiarare la migrazione stabile.

---

## 4. TABELLE — SCHEMA DI RIFERIMENTO COMPLETO

### Istanza `51.83.159.175,1434` (runtime WebApi + Decisore)

```sql
-- Pc_CurrentStatus (preesistente)
-- Campi principali: COMPUTER, ACCOUNT, TAVOLO, STATO, COLORE, ALLARME,
-- SALDO_ISTANTANEO, MARGINE, MEDIA_ORA, COLPO_MARTINGALA, ORE,
-- MAZZO, PBT, LAST_UPDATE, LAST_ADVICE, LAST_INFO, LAST_ACTION, ...

-- Pc_CurrentStatus_PBT_History (creata 2026-05-26)
CREATE TABLE [dbo].[Pc_CurrentStatus_PBT_History] (
    [ID]           BIGINT IDENTITY(1,1) NOT NULL,
    [COMPUTER]     NVARCHAR(50) NOT NULL,
    [PBT]          NVARCHAR(1) NOT NULL,
    [numero_mazzo] NVARCHAR(50) NOT NULL,
    [DT_INSERT]    DATETIME2 NOT NULL,
    CONSTRAINT [PK_Pc_CurrentStatus_PBT_History] PRIMARY KEY ([ID])
);

-- Statistiche (runtime verificato 2026-05-27)
CREATE TABLE dbo.Statistiche (
    ID           BIGINT NOT NULL,
    DATA_INIZIO  DATETIME2 NULL,
    DATA_FINE    DATETIME2 NULL,
    MARGINE_TOT  DECIMAL(18,2) NULL DEFAULT 0,
    MARGINE_MIN  DECIMAL(18,2) NULL DEFAULT 0,
    MARGINE_MAX  DECIMAL(18,2) NULL DEFAULT 0,
    CREATED_AT   DATETIME2 NOT NULL,
    ELAPSED      DECIMAL(18,4) NULL DEFAULT 0,
    TELEMETRY    NVARCHAR(MAX) NULL
);

-- ApiConfigurations (creata 2026-05-26)
CREATE TABLE dbo.ApiConfigurations (
    ID        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pc        NVARCHAR(10) NULL,
    config    NVARCHAR(4000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ApiLogs (creata 2026-05-26)
CREATE TABLE dbo.ApiLogs (
    ID          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Description NVARCHAR(MAX) NULL,
    Category    NVARCHAR(200) NULL,
    Action      INT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

---

## 5. STORED PROCEDURE MANCANTI DA CREARE

### `upI_Values` (mancante su `51.83.159.175,1434`)

Non critica per il Decisore: usata solo in `SaveRequestValue` (fire & forget, errori ignorati).
Prima di crearla: recuperare il corpo da backup affidabile o sorgente storico verificato; non assumere che `1433` sia fonte corretta.

```sql
-- Verifica sul DB runtime
EXEC sp_helptext 'upI_Values';
```

---

## 6. REGOLE ASSOLUTE

1. **Mai DROP TABLE** senza backup e approvazione esplicita.
2. **Mai ALTER COLUMN** su colonne usate da stored procedure senza aggiornare anche le SP.
3. **Mai** eseguire DDL direttamente da SSMS sulla produzione senza il workflow.
4. **Mai** modificare `dbo.Configurations` o `Users_v2` senza review.
5. Script DDL sempre **idempotenti** (`IF NOT EXISTS`).
6. Ogni migrazione ha un **workflow dedicato** con conferma manuale (`workflow_dispatch`).
7. Backup pre-migrazione obbligatorio per tabelle con dati (non per tabelle vuote appena create).
8. Post-migrazione: smoke test applicazione prima di chiudere la sessione.
9. Documentare ogni migrazione nel §11 di `DASH2A-INFRASTRUCTURE.md`.

---

## 7. CHECKLIST RAPIDA

```text
[ ] Confermato target runtime `51.83.159.175,1434`
[ ] Query read-only eseguita — stato attuale noto
[ ] Backup tabella eseguito (se ha dati)
[ ] Script SQL scritto in ops/db-migrations/ e committato
[ ] Workflow creato con conferma manuale
[ ] Workflow eseguito con successo
[ ] POST-CHECK: oggetto esiste, struttura corretta
[ ] Applicazione riavviata e smoke test OK
[ ] DASH2A-INFRASTRUCTURE.md aggiornato
```
