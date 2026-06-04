/*
  Emergency: unlock UserId 13 (Eugenio) — NO password change.
  sqlcmd: -v Confirm="UNLOCK_USER_13"
*/
SET NOCOUNT ON;
IF N'$(Confirm)' <> N'UNLOCK_USER_13'
BEGIN
    RAISERROR('Refusing: Confirm=UNLOCK_USER_13 required.', 16, 1);
    RETURN;
END

UPDATE dbo.Users_v2
SET LockoutEnd = NULL,
    AccessFailedCount = 0,
    EmailConfirmed = 1
WHERE Id = 13;

SELECT Id, UserName, Email, IsRootOwner, LockoutEnd, AccessFailedCount, EmailConfirmed
FROM dbo.Users_v2 WHERE Id = 13;
