# Stage new database connection while the API is offline for the prepared deployment.
$ErrorActionPreference = 'Stop'
if ($env:COMPUTERNAME -ne 'WIN-L28URC6KJHG') { throw 'Wrong server' }
$principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Administrator PowerShell required' }
$root = 'C:\Dash2a\api'
$config = Join-Path $root 'config\appsettings.Production.json'
$offline = Join-Path $root 'current\app_offline.htm'
$backup = Join-Path $root 'config\appsettings.before-fresh-20260925.json'
$db = 'Dash2aFresh20260925'
$result = & sqlcmd.exe -S '.\SQLEXPRESS' -d $db -E -C -b -h -1 -W -Q 'SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0;'
if ($LASTEXITCODE -ne 0 -or ($result -join ' ').Trim() -ne '0') { throw 'Fresh database is not empty; config untouched' }
if (Test-Path -LiteralPath $backup) { throw 'Config backup already exists; refusing repeat' }
$settings = Get-Content -LiteralPath $config -Raw | ConvertFrom-Json
$connection = [string]$settings.ConnectionStrings.DefaultConnection
if ($connection -notmatch '(?i)(^|;)Database=Eugenio-Demo10(?=;|$)') { throw 'Original database setting mismatch; no change' }
if (-not $settings.Database.ApplyMigrationsOnStartup) { throw 'Migration startup flag missing; no change' }
$settings.ConnectionStrings.DefaultConnection = [regex]::Replace($connection, '(?i)(^|;)Database=Eugenio-Demo10(?=;|$)', ('$1Database=' + $db))
Copy-Item -LiteralPath $config -Destination $backup -ErrorAction Stop
Set-Content -LiteralPath $offline -Value 'DASH2A fresh database switch in progress' -Encoding ascii
$tmp = Join-Path (Split-Path -Parent $config) 'appsettings.Production.pending.json'
try {
  $settings | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $tmp -Encoding utf8
  Move-Item -LiteralPath $tmp -Destination $config -Force
  & icacls.exe $config /inheritance:r /grant:r 'BUILTIN\Administrators:F' 'NT AUTHORITY\SYSTEM:F' 'NT AUTHORITY\NETWORK SERVICE:R' 'IIS APPPOOL\DASH2A-API:R' | Out-Null
  if ($LASTEXITCODE -ne 0) { throw 'Config ACL failed; restore backup manually' }
} catch {
  if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Force }
  throw
}
Write-Host 'FRESH_CONFIG_READY: original config backed up; API offline until workflow deploy.'
