/*
  DASH2A — READ ONLY: UserId 13 login diagnostics
*/
SET NOCOUNT ON;

PRINT '=== Users_v2 Id=13 ===';
SELECT
    Id,
    UserName,
    NormalizedUserName,
    Email,
    NormalizedEmail,
    Admin,
    IsRootOwner,
    LockoutEnd,
    LockoutEnabled,
    AccessFailedCount,
    EmailConfirmed,
    CASE WHEN PasswordHash IS NULL OR LEN(PasswordHash) < 10 THEN 'NO_HASH' ELSE 'HAS_HASH' END AS PasswordStatus,
    LEN(PasswordHash) AS PasswordHashLen,
    LastLogin
FROM dbo.Users_v2
WHERE Id = 13;

PRINT '=== Login lookup simulation (UserName) ===';
SELECT Id, UserName, Email, IsRootOwner
FROM dbo.Users_v2
WHERE NormalizedUserName = UPPER(N'Eugenio');

PRINT '=== Login lookup simulation (Email as username — NOT supported by API) ===';
SELECT Id, UserName, Email
FROM dbo.Users_v2
WHERE NormalizedEmail = UPPER(N'Eugenio@dash2a.local');

PRINT '=== AspNetUserRoles Id=13 ===';
SELECT u.UserName, r.Name AS RoleName
FROM dbo.AspNetUserRoles ur
JOIN dbo.Users_v2 u ON u.Id = ur.UserId
JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
WHERE ur.UserId = 13;
