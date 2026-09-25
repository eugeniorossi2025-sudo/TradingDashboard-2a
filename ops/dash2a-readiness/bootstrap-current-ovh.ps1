# One-time elevated bootstrap for the current OVH Windows server.
# Fresh database only. Idempotent for an already-created local DASH2A deployment.
$ErrorActionPreference = 'Stop'
$principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw 'Run from elevated PowerShell on the current OVH server.'
}
$root = 'C:\Dash2a\api'
$current = Join-Path $root 'current'
$configDir = Join-Path $root 'config'
$configFile = Join-Path $configDir 'appsettings.Production.json'
$secretFile = Join-Path $configDir 'bootstrap-admin.txt'
$pool = 'DASH2A-API'
$site = 'DASH2A-API'
$sqlInstance = '.\SQLEXPRESS'
$db = 'Eugenio-Demo10'
$runnerAccount = 'NT AUTHORITY\NETWORK SERVICE'
$appAccount = "IIS APPPOOL\$pool"

Import-Module WebAdministration
if (-not (Get-WebAppPoolState -Name $pool -ErrorAction SilentlyContinue)) {
  New-WebAppPool -Name $pool | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$pool" -Name managedRuntimeVersion -Value ''
Set-ItemProperty "IIS:\AppPools\$pool" -Name processModel.identityType -Value ApplicationPoolIdentity
foreach ($dir in @($root,$current,$configDir)) {
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
}
if (-not (Get-Website -Name $site -ErrorAction SilentlyContinue)) {
  New-Website -Name $site -Port 80 -HostHeader 'api.tradingdash2a.com' -PhysicalPath $current -ApplicationPool $pool | Out-Null
}
$existingSite = Get-Website -Name $site
if ($existingSite.PhysicalPath -ne $current) { throw "Existing site uses another path: $($existingSite.PhysicalPath)" }
$bindings = @($existingSite.Bindings.Collection | ForEach-Object { "$($_.protocol):$($_.bindingInformation)" })
if ($bindings -notcontains 'http:*:80:api.tradingdash2a.com') {
  throw 'Existing IIS site lacks expected api.tradingdash2a.com HTTP binding.'
}

& icacls.exe $root /grant "$($runnerAccount):(OI)(CI)M" "$($appAccount):(OI)(CI)RX" | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Failed to grant release-folder access' }
& icacls.exe $configDir /inheritance:r /grant:r 'BUILTIN\Administrators:(OI)(CI)F' 'NT AUTHORITY\SYSTEM:(OI)(CI)F' "$($runnerAccount):(OI)(CI)RX" "$($appAccount):(OI)(CI)RX" | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Failed to restrict configuration-folder access' }

$query = @"
IF DB_ID(N'$db') IS NULL CREATE DATABASE [$db];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'$appAccount')
  CREATE LOGIN [$appAccount] FROM WINDOWS;
USE [$db];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'$appAccount')
  CREATE USER [$appAccount] FOR LOGIN [$appAccount];
IF IS_ROLEMEMBER(N'db_owner',N'$appAccount') <> 1
  ALTER ROLE db_owner ADD MEMBER [$appAccount];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'NT AUTHORITY\NETWORK SERVICE')
  CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];
IF IS_ROLEMEMBER(N'db_datareader',N'NT AUTHORITY\NETWORK SERVICE') <> 1
  ALTER ROLE db_datareader ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
SELECT name,state_desc FROM sys.databases WHERE name=N'$db';
"@
& sqlcmd.exe -S $sqlInstance -E -C -b -Q $query -W
if ($LASTEXITCODE -ne 0) { throw 'SQL bootstrap failed; IIS setup was completed but no deployment attempted.' }

if (-not (Test-Path -LiteralPath $configFile)) {
  $jwtBytes = New-Object byte[] 48
  $adminBytes = New-Object byte[] 36
  $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
  try { $rng.GetBytes($jwtBytes); $rng.GetBytes($adminBytes) } finally { $rng.Dispose() }
  $adminPassword = [Convert]::ToBase64String($adminBytes)
  $settings = [ordered]@{
    ConnectionStrings = @{ DefaultConnection = 'Server=.\SQLEXPRESS;Database=Eugenio-Demo10;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;' }
    Database = @{ ApplyMigrationsOnStartup = $true }
    Jwt = @{ Key = [Convert]::ToBase64String($jwtBytes); Issuer = 'WebApi'; Audience = 'WebApiUsers'; ExpirationMinutes = '60' }
    Admin = @{ Username = 'admin'; Password = $adminPassword; Email = 'admin@botdashboard.local' }
    AllowedHosts = '*'
  }
  $settings | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $configFile -Encoding utf8
  "Generated admin password for the new empty database: $adminPassword" | Set-Content -LiteralPath $secretFile -Encoding utf8
}
foreach ($file in @($configFile,$secretFile)) {
  & icacls.exe $file /inheritance:r /grant:r 'BUILTIN\Administrators:F' 'NT AUTHORITY\SYSTEM:F' | Out-Null
  if ($file -eq $configFile) {
    & icacls.exe $file /grant "$($runnerAccount):R" "$($appAccount):R" | Out-Null
  }
  if ($LASTEXITCODE -ne 0) { throw "Failed to protect $file" }
}
Write-Host 'BOOTSTRAP_PASS: empty local SQL DB, dedicated IIS site and protected local configuration ready.'
Write-Host 'Admin password saved on server for Administrator only; it was not printed.'
Write-Host 'No backend deploy, HTTPS binding or firewall change performed by bootstrap.'



