/*
  DASH2A — READ ONLY: identify real login account vs bootstrap UserId 13
*/
SET NOCOUNT ON;

PRINT '=== Users matching eugenio (name/email) ===';
SELECT Id, UserName, NormalizedUserName, Email, NormalizedEmail, Admin, IsRootOwner, LockoutEnd, LastLogin
FROM dbo.Users_v2
WHERE UserName LIKE '%eugenio%'
   OR Email LIKE '%eugenio%'
   OR NormalizedUserName LIKE '%EUGENIO%'
   OR NormalizedEmail LIKE '%EUGENIO%'
ORDER BY Id;

PRINT '=== Current Root Owner(s) ===';
SELECT Id, UserName, Email, Admin, IsRootOwner, LockoutEnd, LastLogin
FROM dbo.Users_v2
WHERE IsRootOwner = 1
ORDER BY Id;

PRINT '=== Admin users by LastLogin (recent first) ===';
SELECT TOP 30
    Id, UserName, Email, Admin, IsRootOwner, LastLogin,
    CASE WHEN PasswordHash IS NULL OR LEN(PasswordHash) < 10 THEN 'NO_HASH' ELSE 'HAS_HASH' END AS PasswordStatus
FROM dbo.Users_v2
WHERE Admin = 1
ORDER BY LastLogin DESC;

PRINT '=== All users with recent LastLogin (top 40) ===';
SELECT TOP 40
    Id, UserName, Email, Admin, IsRootOwner, LastLogin
FROM dbo.Users_v2
WHERE LastLogin IS NOT NULL
ORDER BY LastLogin DESC;

PRINT '=== UserAccessEvents: recent LOGIN (top 50) ===';
IF OBJECT_ID(N'dbo.UserAccessEvents', N'U') IS NOT NULL
BEGIN
    SELECT TOP 50
        e.ID,
        e.UserId,
        e.Username,
        e.EventType,
        e.Page,
        LEFT(e.UserAgent, 80) AS UserAgentShort,
        e.OccurredAtUtc
    FROM dbo.UserAccessEvents e
    WHERE e.EventType = 'LOGIN'
    ORDER BY e.OccurredAtUtc DESC;
END
ELSE
    PRINT 'UserAccessEvents table not found';

PRINT '=== UserAccessEvents: mobile-related (top 40) ===';
IF OBJECT_ID(N'dbo.UserAccessEvents', N'U') IS NOT NULL
BEGIN
    SELECT TOP 40
        e.UserId,
        e.Username,
        e.EventType,
        e.Page,
        e.OccurredAtUtc
    FROM dbo.UserAccessEvents e
    WHERE e.Page LIKE '%mobile%'
       OR e.UserAgent LIKE '%Mobile%'
       OR e.UserAgent LIKE '%Android%'
       OR e.UserAgent LIKE '%iPhone%'
    ORDER BY e.OccurredAtUtc DESC;
END

PRINT '=== UserAccessEvents: counts by UserId (last 30 days) ===';
IF OBJECT_ID(N'dbo.UserAccessEvents', N'U') IS NOT NULL
BEGIN
    SELECT TOP 25
        e.UserId,
        u.UserName,
        u.Email,
        u.Admin,
        COUNT(*) AS EventCount,
        SUM(CASE WHEN e.EventType = 'LOGIN' THEN 1 ELSE 0 END) AS LoginCount,
        MAX(e.OccurredAtUtc) AS LastEventUtc
    FROM dbo.UserAccessEvents e
    LEFT JOIN dbo.Users_v2 u ON u.Id = e.UserId
    WHERE e.OccurredAtUtc >= DATEADD(day, -30, SYSUTCDATETIME())
    GROUP BY e.UserId, u.UserName, u.Email, u.Admin
    ORDER BY LastEventUtc DESC;
END

PRINT '=== AspNetUserRoles for eugenio-like users ===';
SELECT u.Id, u.UserName, u.Email, r.Name AS RoleName
FROM dbo.Users_v2 u
LEFT JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
WHERE u.UserName LIKE '%eugenio%'
   OR u.Email LIKE '%eugenio%'
   OR u.NormalizedUserName LIKE '%EUGENIO%'
   OR u.NormalizedEmail LIKE '%EUGENIO%'
ORDER BY u.Id, r.Name;

PRINT '=== Users_v2 Description field (bot/demo hints) ===';
SELECT Id, UserName, Email, Admin, IsRootOwner, Description, LastLogin
FROM dbo.Users_v2
WHERE Description IS NOT NULL AND LTRIM(RTRIM(Description)) <> ''
ORDER BY Id;
