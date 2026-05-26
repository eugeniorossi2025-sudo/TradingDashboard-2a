-- =============================================================================
-- migrate-decisore-schema-v1-safe.sql
-- DB: Eugenio-Demo10 (VPS Decisore 51.178.16.37)
--
-- SAFE / IDEMPOTENTE:
-- - Crea dbo.Statistiche solo se manca.
-- - NON droppa procedure esistenti.
-- - Crea dbo.AggiornaStatistiche solo se manca.
-- - Crea dbo.InsertMargine solo se manca.
-- - Non inserisce righe reali in dbo.Margini.
-- - Stampa stato e definizioni correnti per audit manuale.
-- =============================================================================

USE [Eugenio-Demo10];
GO

PRINT '=== AUDIT: procedure esistenti ===';
GO

IF OBJECT_ID(N'dbo.AggiornaStatistiche', N'P') IS NOT NULL
BEGIN
    PRINT 'EXISTS: dbo.AggiornaStatistiche - definition follows';
    EXEC sp_helptext N'dbo.AggiornaStatistiche';
END
ELSE
BEGIN
    PRINT 'MISSING: dbo.AggiornaStatistiche';
END
GO

IF OBJECT_ID(N'dbo.InsertMargine', N'P') IS NOT NULL
BEGIN
    PRINT 'EXISTS: dbo.InsertMargine - definition follows';
    EXEC sp_helptext N'dbo.InsertMargine';
END
ELSE
BEGIN
    PRINT 'MISSING: dbo.InsertMargine';
END
GO

PRINT '=== MIGRATION SAFE: dbo.Statistiche ===';
GO

IF OBJECT_ID(N'dbo.Statistiche', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Statistiche
    (
        ID          BIGINT        IDENTITY(1,1) NOT NULL,
        DATA_INIZIO DATETIME2     NOT NULL,
        DATA_FINE   DATETIME2     NULL,
        MARGINE_TOT DECIMAL(19,0) NOT NULL CONSTRAINT DF_Statistiche_MARGINE_TOT DEFAULT (0),
        MARGINE_MIN DECIMAL(19,0) NOT NULL CONSTRAINT DF_Statistiche_MARGINE_MIN DEFAULT (0),
        MARGINE_MAX DECIMAL(19,0) NOT NULL CONSTRAINT DF_Statistiche_MARGINE_MAX DEFAULT (0),
        CREATED_AT  DATETIME2     NOT NULL CONSTRAINT DF_Statistiche_CREATED_AT DEFAULT (SYSUTCDATETIME()),
        ELAPSED     DECIMAL(10,0) NOT NULL CONSTRAINT DF_Statistiche_ELAPSED DEFAULT (0),
        TELEMETRY   NVARCHAR(MAX) NULL,
        CONSTRAINT PK_Statistiche PRIMARY KEY (ID)
    );

    PRINT 'CREATED: dbo.Statistiche';
END
ELSE
BEGIN
    PRINT 'OK EXISTS: dbo.Statistiche - no schema changes applied';
END
GO

PRINT '=== MIGRATION SAFE: dbo.AggiornaStatistiche ===';
GO

IF OBJECT_ID(N'dbo.AggiornaStatistiche', N'P') IS NULL
BEGIN
    EXEC(N'
CREATE PROCEDURE dbo.AggiornaStatistiche
    @TELEMETRY NVARCHAR(MAX),
    @ELAPSED   DECIMAL(10,0)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MargineTot DECIMAL(19,0) = 0;
    DECLARE @CurMin DECIMAL(19,0);
    DECLARE @CurMax DECIMAL(19,0);

    SELECT @MargineTot = ISNULL(SUM(MARGINE), 0)
    FROM dbo.Pc_CurrentStatus;

    SELECT @CurMin = MARGINE_MIN,
           @CurMax = MARGINE_MAX
    FROM dbo.Statistiche
    WHERE DATA_FINE IS NULL;

    SET @CurMin = CASE
        WHEN @CurMin IS NULL OR @MargineTot < @CurMin THEN @MargineTot
        ELSE @CurMin
    END;

    SET @CurMax = CASE
        WHEN @CurMax IS NULL OR @MargineTot > @CurMax THEN @MargineTot
        ELSE @CurMax
    END;

    UPDATE dbo.Statistiche
    SET TELEMETRY = @TELEMETRY,
        ELAPSED = @ELAPSED,
        MARGINE_TOT = @MargineTot,
        MARGINE_MIN = @CurMin,
        MARGINE_MAX = @CurMax
    WHERE DATA_FINE IS NULL;
END
');

    PRINT 'CREATED: dbo.AggiornaStatistiche';
END
ELSE
BEGIN
    PRINT 'OK EXISTS: dbo.AggiornaStatistiche - no changes applied';
END
GO

PRINT '=== MIGRATION SAFE: dbo.InsertMargine ===';
GO

IF OBJECT_ID(N'dbo.InsertMargine', N'P') IS NULL
BEGIN
    EXEC(N'
CREATE PROCEDURE dbo.InsertMargine
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Margine DECIMAL(18,0);

    SELECT @Margine = ISNULL(SUM(MARGINE), 0)
    FROM dbo.Pc_CurrentStatus;

    BEGIN TRY
        INSERT INTO dbo.Margini (Margine, Data)
        VALUES (@Margine, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        INSERT INTO dbo.Margini (Id, Margine, Data)
        VALUES (ISNULL((SELECT MAX(Id) FROM dbo.Margini), 0) + 1,
                @Margine,
                SYSUTCDATETIME());
    END CATCH
END
');

    PRINT 'CREATED: dbo.InsertMargine';
END
ELSE
BEGIN
    PRINT 'OK EXISTS: dbo.InsertMargine - no changes applied';
END
GO

PRINT '=== VERIFICATION: dbo.Statistiche schema ===';
GO

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    COLUMNPROPERTY(OBJECT_ID(N'dbo.Statistiche'), COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Statistiche'
ORDER BY ORDINAL_POSITION;
GO

PRINT '=== VERIFICATION: procedure status ===';
GO

SELECT
    name,
    type_desc,
    create_date,
    modify_date
FROM sys.objects
WHERE object_id IN
(
    OBJECT_ID(N'dbo.AggiornaStatistiche', N'P'),
    OBJECT_ID(N'dbo.InsertMargine', N'P')
)
ORDER BY name;
GO

PRINT '=== migrate-decisore-schema-v1-safe.sql completed ===';
GO
