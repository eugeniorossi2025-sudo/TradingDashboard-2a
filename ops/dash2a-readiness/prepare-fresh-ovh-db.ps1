# One-time fresh database preparation. Existing DB and IIS configuration stay untouched.
$ErrorActionPreference = 'Stop'
if ($env:COMPUTERNAME -ne 'WIN-L28URC6KJHG') { throw 'Wrong server' }
$principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Administrator PowerShell required' }
$db = 'Dash2aFresh20260925'
$poolLogin = 'IIS APPPOOL\DASH2A-API'
$runnerLogin = 'NT AUTHORITY\NETWORK SERVICE'
$exists = & sqlcmd.exe -S '.\SQLEXPRESS' -d master -E -C -b -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$db') IS NULL THEN 'ABSENT' ELSE 'PRESENT' END"
if ($LASTEXITCODE -ne 0 -or ($exists -join ' ').Trim() -ne 'ABSENT') { throw 'Fresh database already exists or preflight failed; no changes made' }
& sqlcmd.exe -S '.\SQLEXPRESS' -d master -E -C -b -Q "CREATE DATABASE [$db]"
if ($LASTEXITCODE -ne 0) { throw 'Fresh database creation failed' }
$logins = @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'$poolLogin')
  EXEC(N'CREATE LOGIN [$poolLogin] FROM WINDOWS');
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'$runnerLogin')
  EXEC(N'CREATE LOGIN [$runnerLogin] FROM WINDOWS');
"@
& sqlcmd.exe -S '.\SQLEXPRESS' -d master -E -C -b -Q $logins
if ($LASTEXITCODE -ne 0) { throw 'SQL login setup failed; old database untouched' }
$grants = @"
CREATE USER [$poolLogin] FOR LOGIN [$poolLogin];
ALTER ROLE db_owner ADD MEMBER [$poolLogin];
CREATE USER [$runnerLogin] FOR LOGIN [$runnerLogin];
ALTER ROLE db_datareader ADD MEMBER [$runnerLogin];
SELECT DB_NAME() AS FreshDatabase, COUNT(*) AS UserTables FROM sys.tables WHERE is_ms_shipped=0;
"@
& sqlcmd.exe -S '.\SQLEXPRESS' -d $db -E -C -b -Q $grants -W
if ($LASTEXITCODE -ne 0) { throw 'SQL permission setup failed; old database untouched' }
Write-Host 'FRESH_DB_PASS: new empty database ready; old database and IIS configuration untouched.'
