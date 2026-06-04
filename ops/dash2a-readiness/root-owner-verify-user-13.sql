/*
  DASH2A Root Owner — READ ONLY verify UserId 13 before bootstrap
*/
SET NOCOUNT ON;

PRINT '=== Users_v2 Id=13 (pre-bootstrap) ===';

IF COL_LENGTH('dbo.Users_v2', 'IsRootOwner') IS NULL
    PRINT 'NOTE: IsRootOwner column not present yet — bootstrap script will add it.';
ELSE
    SELECT Id, UserName, Email, Admin, IsRootOwner, LockoutEnd
    FROM dbo.Users_v2
    WHERE Id = 13;
GO

IF COL_LENGTH('dbo.Users_v2', 'IsRootOwner') IS NOT NULL
BEGIN
    SELECT Id, UserName, Email, IsRootOwner
    FROM dbo.Users_v2
    WHERE Id = 13;
END
ELSE
BEGIN
    SELECT Id, UserName, Email, Admin
    FROM dbo.Users_v2
    WHERE Id = 13;
END

PRINT '=== Current root owners (if column exists) ===';
IF COL_LENGTH('dbo.Users_v2', 'IsRootOwner') IS NOT NULL
    SELECT Id, UserName, IsRootOwner FROM dbo.Users_v2 WHERE IsRootOwner = 1;
ELSE
    PRINT '(none — column missing)';
