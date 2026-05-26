-- =============================================================================
-- migrate-decisore-schema-v1.sql
-- DB: Eugenio-Demo10  (VPS Decisore 51.178.16.37)
-- Idempotente: sicuro da rieseguire più volte senza effetti collaterali.
-- Crea: dbo.Statistiche, dbo.AggiornaStatistiche, dbo.InsertMargine
-- NON tocca: dbo.Margini, dbo.Pc_CurrentStatus_PBT_History (già corrette)
-- =============================================================================

USE [Eugenio-Demo10];
GO

-- ---------------------------------------------------------------------------
-- 1. dbo.Statistiche
--    ID       : BIGINT IDENTITY(1,1) — auto-generato, non passare nelle INSERT
--    CREATED_AT: DEFAULT SYSUTCDATETIME() — auto-generato
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Statistiche', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Statistiche (
        ID          BIGINT          IDENTITY(1,1) NOT NULL,
        DATA_INIZIO DATETIME2       NOT NULL,
        DATA_FINE   DATETIME2       NULL,
        MARGINE_TOT DECIMAL(19,0)   NOT NULL DEFAULT 0,
        MARGINE_MIN DECIMAL(19,0)   NOT NULL DEFAULT 0,
        MARGINE_MAX DECIMAL(19,0)   NOT NULL DEFAULT 0,
        CREATED_AT  DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
        ELAPSED     DECIMAL(10,0)   NOT NULL DEFAULT 0,
        TELEMETRY   NVARCHAR(MAX)   NULL,
        CONSTRAINT PK_Statistiche PRIMARY KEY (ID)
    );
    PRINT 'CREATED: dbo.Statistiche';
END
ELSE
BEGIN
    PRINT 'OK (exists): dbo.Statistiche';
END
GO

-- ---------------------------------------------------------------------------
-- 2. dbo.AggiornaStatistiche
--    Chiamata dopo ogni /decide con @TELEMETRY e @ELAPSED.
--    Aggiorna la sessione aperta (DATA_FINE IS NULL):
--      - telemetry, elapsed
--      - MARGINE_TOT = somma MARGINE da Pc_CurrentStatus
--      - MARGINE_MIN / MARGINE_MAX rolling sulla sessione corrente
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.AggiornaStatistiche', N'P') IS NOT NULL
    DROP PROCEDURE dbo.AggiornaStatistiche;
GO

CREATE PROCEDURE dbo.AggiornaStatistiche
    @TELEMETRY  NVARCHAR(MAX),
    @ELAPSED    DECIMAL(10,0)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MargineTot DECIMAL(19,0) = 0;
    DECLARE @CurMin     DECIMAL(19,0);
    DECLARE @CurMax     DECIMAL(19,0);

    -- Margine totale istantaneo da tutti i PC attivi
    SELECT @MargineTot = ISNULL(SUM(MARGINE), 0)
    FROM   dbo.Pc_CurrentStatus;

    -- Min/Max della sessione corrente (prima dell'aggiornamento)
    SELECT @CurMin = MARGINE_MIN,
           @CurMax = MARGINE_MAX
    FROM   dbo.Statistiche
    WHERE  DATA_FINE IS NULL;

    -- Rolling min/max
    SET @CurMin = CASE
                    WHEN @CurMin IS NULL OR @MargineTot < @CurMin THEN @MargineTot
                    ELSE @CurMin
                  END;
    SET @CurMax = CASE
                    WHEN @CurMax IS NULL OR @MargineTot > @CurMax THEN @MargineTot
                    ELSE @CurMax
                  END;

    UPDATE dbo.Statistiche
    SET    TELEMETRY   = @TELEMETRY,
           ELAPSED     = @ELAPSED,
           MARGINE_TOT = @MargineTot,
           MARGINE_MIN = @CurMin,
           MARGINE_MAX = @CurMax
    WHERE  DATA_FINE IS NULL;
END
GO

PRINT 'CREATED/REPLACED: dbo.AggiornaStatistiche';

-- ---------------------------------------------------------------------------
-- 3. dbo.InsertMargine
--    Nessun parametro. Snapshot del margine totale in dbo.Margini.
--    Gestisce sia Id IDENTITY che Id esplicito (TRY/CATCH).
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.InsertMargine', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InsertMargine;
GO

CREATE PROCEDURE dbo.InsertMargine
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Margine DECIMAL(18,0);
    SELECT @Margine = ISNULL(SUM(MARGINE), 0) FROM dbo.Pc_CurrentStatus;

    BEGIN TRY
        -- Tenta INSERT senza Id (funziona se la colonna è IDENTITY)
        INSERT INTO dbo.Margini (Margine, Data)
        VALUES (@Margine, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        -- Fallback: Id esplicito (se la colonna NON è IDENTITY)
        INSERT INTO dbo.Margini (Id, Margine, Data)
        VALUES (ISNULL((SELECT MAX(Id) FROM dbo.Margini), 0) + 1,
                @Margine,
                SYSUTCDATETIME());
    END CATCH
END
GO

PRINT 'CREATED/REPLACED: dbo.InsertMargine';

-- ---------------------------------------------------------------------------
-- FINE SCRIPT
-- ---------------------------------------------------------------------------
PRINT '=== migrate-decisore-schema-v1.sql completato ===';
GO
