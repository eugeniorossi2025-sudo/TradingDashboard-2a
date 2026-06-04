/*
  DASH2A Root Owner — READ ONLY verify UserId 13 before bootstrap
  (Does not reference IsRootOwner — column may not exist yet.)
*/
SET NOCOUNT ON;

PRINT '=== Users_v2 Id=13 (pre-bootstrap) ===';

IF NOT EXISTS (SELECT 1 FROM dbo.Users_v2 WHERE Id = 13)
BEGIN
    RAISERROR('UserId 13 not found in Users_v2 — abort bootstrap.', 16, 1);
    RETURN;
END

SELECT Id, UserName, Email, Admin, LockoutEnd
FROM dbo.Users_v2
WHERE Id = 13;
