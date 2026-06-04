/*
  DASH2A — Backup righe MissionSessions #101-#104 prima della bonifica temporale.
  Eseguire SUBITO prima di 03-apply-overlap-time-recovery.sql
*/
SET NOCOUNT ON;

DECLARE @BackupTable sysname = N'MissionSessions_bkp_overlap_time_' + CONVERT(varchar(8), GETUTCDATE(), 112)
    + N'_' + REPLACE(CONVERT(varchar(8), GETUTCDATE(), 108), ':', '');
DECLARE @sql nvarchar(max);

SET @sql = N'SELECT * INTO dbo.' + QUOTENAME(@BackupTable) + N' FROM dbo.MissionSessions WHERE ID IN (101, 102, 103, 104);';
EXEC sp_executesql @sql;

DECLARE @rows bigint;
SET @sql = N'SELECT @rows = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@BackupTable) + N';';
EXEC sp_executesql @sql, N'@rows bigint OUTPUT', @rows = @rows OUTPUT;

PRINT 'BACKUP_TABLE=' + @BackupTable;
PRINT 'BACKUP_ROWS=' + CAST(@rows AS varchar(20));

SELECT *
FROM dbo.MissionSessions
WHERE ID IN (101, 102, 103, 104)
ORDER BY ID;
