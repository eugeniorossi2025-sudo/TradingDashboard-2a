SET NOCOUNT ON;
SELECT Id, UserName, NormalizedUserName, Email, Admin, IsRootOwner, LockoutEnd, LockoutEnabled, AccessFailedCount, EmailConfirmed,
  CASE WHEN PasswordHash IS NULL OR LEN(PasswordHash)<10 THEN 'NO' ELSE 'YES' END AS HasHash
FROM dbo.Users_v2 WHERE Id = 13 OR UserName = N'Eugenio';
