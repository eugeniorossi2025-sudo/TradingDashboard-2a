/*
  DASH2A Root Owner — READ ONLY verify UserId 13 before bootstrap
*/
SET NOCOUNT ON;

PRINT '=== Users_v2 Id=13 (pre-bootstrap) ===';

IF NOT EXISTS (SELECT 1 FROM dbo.Users_v2 WHERE Id = 13)
BEGIN
    RAISERROR('UserId 13 not found in Users_v2 — abort bootstrap.', 16, 1);
    RETURN;
END

IF COL_LENGTH('dbo.Users_v2', 'IsRootOwner') IS NULL
BEGIN
    PRINT 'NOTE: IsRootOwner column not present yet — bootstrap will add it.';
    SELECT Id, UserName, Email, Admin, LockoutEnd
    FROM dbo.Users_v2
    WHERE Id = 13;
END
ELSE
BEGIN
    SELECT Id, UserName, Email, Admin, IsRootOwner, LockoutEnd
    FROM dbo.Users_v2
    WHERE Id = 13;

    SELECT Id, UserName, IsRootOwner
    FROM dbo.Users_v2
    WHERE IsRootOwner = 1;
END
