-- =============================================================================
-- audit-decisore-procedures.sql
-- DB: Eugenio-Demo10 (VPS Decisore 51.178.16.37)
--
-- Read-only audit. Does not create, update, drop, or insert anything.
-- Use before applying migrate-decisore-schema-v1-safe.sql.
-- =============================================================================

USE [Eugenio-Demo10];
GO

PRINT '=== OBJECT EXISTENCE ===';
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
    OBJECT_ID(N'dbo.InsertMargine', N'P'),
    OBJECT_ID(N'dbo.Statistiche', N'U')
)
ORDER BY type_desc, name;
GO

PRINT '=== dbo.AggiornaStatistiche definition ===';
GO

IF OBJECT_ID(N'dbo.AggiornaStatistiche', N'P') IS NOT NULL
    EXEC sp_helptext N'dbo.AggiornaStatistiche';
ELSE
    PRINT 'MISSING: dbo.AggiornaStatistiche';
GO

PRINT '=== dbo.InsertMargine definition ===';
GO

IF OBJECT_ID(N'dbo.InsertMargine', N'P') IS NOT NULL
    EXEC sp_helptext N'dbo.InsertMargine';
ELSE
    PRINT 'MISSING: dbo.InsertMargine';
GO

PRINT '=== dbo.Statistiche schema, if present ===';
GO

IF OBJECT_ID(N'dbo.Statistiche', N'U') IS NOT NULL
BEGIN
    SELECT
        COLUMN_NAME,
        DATA_TYPE,
        COLUMNPROPERTY(OBJECT_ID(N'dbo.Statistiche'), COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Statistiche'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'MISSING: dbo.Statistiche';
END
GO

PRINT '=== audit-decisore-procedures.sql completed ===';
GO
