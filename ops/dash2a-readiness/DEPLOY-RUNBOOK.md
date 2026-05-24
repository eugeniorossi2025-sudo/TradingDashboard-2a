# DASH2A Deploy Runbook

This runbook is mandatory before any DASH2A backend deploy.

Current state: backend deploy is blocked until all TODO placeholders are resolved and backups are verified.

Known OVH target from panel:
- Service name: `Back-end Dashboard`
- Hostname: `vps-4ca306e8.vps.ovh.net`
- IPv4: `51.83.159.175`
- OS: `Windows Server 2025 Standard (Desktop)`
- Region: `os-waw2`, Warsaw (WAW), Poland
- Backup: automatic backup Standard, last reported `2026-05-23 21:17`; snapshot disabled.
- RDP prerequisite: user `administrator`, password stored as GitHub Actions secret `DASH2A_RDP_PASSWORD`.

The OVH automatic backup does not replace app-level and DB-level backups for deploy.

The RDP secret is for controlled remote access readiness only. Do not print it, pass it on a command line, write it to logs, or use it for unprotected automatic deploys.

## 0. Scope

Allowed only after explicit user approval:
- Merge PR.
- Firebase frontend deploy.
- Backend deploy.
- Server access.

Not allowed during readiness:
- No server changes.
- No production restart.
- No DB migration on production.
- No Firebase deploy.

## 1. Pre-Deploy Checklist

Complete every item before deploy.

- [ ] Complete read-only server audit first.
- [ ] Confirm repository is `TradingDashboard-2a`.
- [ ] Confirm branch/commit to deploy.
- [ ] Confirm Dashboard 1 is not referenced.
- [ ] Confirm Firebase project is `eugenio-dashboard-2`.
- [ ] Confirm backend/API target is the intended DASH2A server.
- [ ] Confirm server identity and OS.
- [ ] Confirm app process: IIS App Pool, Windows Service, or other.
- [ ] Confirm app deploy folder.
- [ ] Confirm logs folder.
- [ ] Confirm DB host, DB name, and DB user.
- [ ] Confirm server env/secrets are configured.
- [ ] Confirm tracked `appsettings.json` files contain no real secrets.
- [ ] Confirm DB backup command has been tested.
- [ ] Confirm app backup command has been tested.
- [ ] Confirm rollback command has been tested or rehearsed.
- [ ] Confirm smoke test endpoints.
- [ ] Confirm migration script is reviewed.

## 1A. Read-Only Server Audit Commands

These commands are for identification only and require explicit server access approval. Do not run restart, deploy, migration, backup, restore, or config-edit commands in this phase.

## 1B. GitHub Actions Self-Hosted Runner Setup

Use this only when the user explicitly authorizes installing the runner service on the DASH2A VPS.

Purpose:
- Install GitHub Actions self-hosted runner as a Windows Service.
- Enable future read-only inventory workflows.
- Do not deploy.
- Do not restart IIS.
- Do not restart the server.
- Do not change firewall rules.
- Do not modify `C:\inetpub\wwwroot\publish`.
- Do not change IIS bindings or App Pools.

Required values:
- Repo: `https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a`
- Runner name: `dash2a-windows-runner-01`
- Runner root: `C:\actions-runner`
- Runner directory: `C:\actions-runner\dash2a-windows-runner-01`
- Custom label: `DASH2A`
- Automatic labels expected from GitHub: `self-hosted`, `Windows`, `X64`

Token handling:
- Generate a new registration token from GitHub immediately before installation.
- Treat any earlier token as exposed.
- Do not hardcode the token in scripts.
- Do not pass the token through chat.
- Do not print the token.
- Do not save the token to files.
- Paste it only into the interactive prompt on the VPS.

Run on the VPS:
- Open PowerShell as Administrator.
- Run `ops\dash2a-readiness\install-dash2a-runner.ps1` or paste its contents into the elevated PowerShell session.
- When prompted, paste a fresh GitHub runner registration token.

Post-setup verification from local machine:

```powershell
gh api repos/eugeniorossi2025-sudo/TradingDashboard-2a/actions/runners `
  --jq '.runners[]? | [.name,.os,.status,.busy,(.labels|map(.name)|join(","))] | @tsv'
```

Expected:
- `dash2a-windows-runner-01`
- `online`
- `busy=false`
- labels include `self-hosted`, `Windows`, `DASH2A`

Windows/IIS discovery:

```powershell
$ErrorActionPreference = 'Stop'

Write-Host '--- OS ---'
Get-ComputerInfo | Select-Object CsName,WindowsProductName,WindowsVersion,OsHardwareAbstractionLayer

Write-Host '--- IIS feature ---'
Get-WindowsFeature Web-Server -ErrorAction SilentlyContinue

Write-Host '--- IIS sites/app pools ---'
Import-Module WebAdministration
Get-Website | Select-Object Name,State,PhysicalPath,Bindings
Get-ChildItem IIS:\AppPools | Select-Object Name,State,managedRuntimeVersion

Write-Host '--- Listening ports ---'
Get-NetTCPConnection -State Listen | Select-Object LocalAddress,LocalPort,OwningProcess | Sort-Object LocalPort

Write-Host '--- Environment hints ---'
[Environment]::GetEnvironmentVariables('Machine').Keys | Sort-Object | Select-String -Pattern 'ASPNETCORE|ConnectionStrings|Jwt|Smtp|DASH2A|Dashboard'
```

Linux/systemd discovery:

```bash
set -euo pipefail

echo '--- OS ---'
uname -a
cat /etc/os-release || true

echo '--- Services ---'
systemctl list-units --type=service --no-pager | grep -Ei 'dash|webapi|dotnet|nginx|apache|kestrel|eugenio' || true

echo '--- Listening ports ---'
ss -ltnp || netstat -ltnp || true

echo '--- Reverse proxy candidates ---'
ls -la /etc/nginx/sites-enabled /etc/nginx/conf.d 2>/dev/null || true

echo '--- Environment hints ---'
env | grep -Ei 'ASPNETCORE|ConnectionStrings|Jwt|Smtp|DASH2A|Dashboard' || true
```

SQL read-only discovery:

```sql
SELECT @@SERVERNAME AS ServerName, DB_NAME() AS CurrentDatabase, SUSER_SNAME() AS LoginName;

SELECT name
FROM sys.databases
ORDER BY name;

SELECT HAS_DBACCESS('TODO_DASH2A_DATABASE') AS HasDash2aDbAccess;

IF OBJECT_ID('__EFMigrationsHistory') IS NOT NULL
BEGIN
    SELECT MigrationId, ProductVersion
    FROM __EFMigrationsHistory
    ORDER BY MigrationId;
END;
```

## 2. Freeze Window

Before deploy:
- Stop new feature work.
- Stop manual changes on server.
- Record current commit deployed: `TODO`.
- Record current DB backup label: `TODO`.
- Record current app backup label: `TODO`.

## 3. Backup App

Use the server-specific command after app folder is identified.

PowerShell template for Windows/IIS:

```powershell
$ErrorActionPreference = 'Stop'
$Timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$AppPath = 'TODO_APP_PATH'
$BackupRoot = 'TODO_BACKUP_ROOT'
$BackupPath = Join-Path $BackupRoot "app-$Timestamp"

if (-not (Test-Path -LiteralPath $AppPath)) { throw "Missing app path: $AppPath" }
New-Item -ItemType Directory -Force -Path $BackupPath | Out-Null
Copy-Item -LiteralPath $AppPath -Destination $BackupPath -Recurse -Force
Get-ChildItem -LiteralPath $BackupPath -Force | Select-Object -First 20 FullName,Length,LastWriteTime
Write-Host "APP_BACKUP=$BackupPath"
```

Linux/systemd template, if server is Linux:

```bash
set -euo pipefail
timestamp="$(date +%Y%m%d-%H%M%S)"
app_path="TODO_APP_PATH"
backup_root="TODO_BACKUP_ROOT"
backup_path="$backup_root/app-$timestamp"

test -d "$app_path"
mkdir -p "$backup_path"
cp -a "$app_path/." "$backup_path/"
find "$backup_path" -maxdepth 2 -type f | head -50
echo "APP_BACKUP=$backup_path"
```

## 4. Backup DB

SQL Server backup template. Use a folder that SQL Server can write to.

```sql
DECLARE @BackupPath nvarchar(4000);
SET @BackupPath = N'TODO_SQL_BACKUP_PATH\DASH2A_' +
    CONVERT(varchar(8), GETDATE(), 112) + '_' +
    REPLACE(CONVERT(varchar(8), GETDATE(), 108), ':', '') + '.bak';

BACKUP DATABASE [TODO_DATABASE_NAME]
TO DISK = @BackupPath
WITH COPY_ONLY, INIT, COMPRESSION, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY
FROM DISK = @BackupPath
WITH CHECKSUM;

SELECT @BackupPath AS BackupPath;
```

If direct SQL backup is not available, stop. Do not deploy until an equivalent tested DB backup exists.

## 5. Publish Artifact

Build from a clean checkout of the approved commit.

```powershell
dotnet restore .\backend\WebApi\WebApi.csproj
dotnet build .\backend\WebApi\WebApi.csproj -c Release --no-restore
dotnet publish .\backend\WebApi\WebApi.csproj -c Release -o .\artifacts\dash2a-webapi
```

Validate artifact:

```powershell
Test-Path .\artifacts\dash2a-webapi\WebApi.dll
Get-ChildItem .\artifacts\dash2a-webapi | Select-Object Name,Length,LastWriteTime
```

## 6. Deploy Backend

Do not run until server process model is known.

Windows/IIS template:

```powershell
$ErrorActionPreference = 'Stop'
$PublishPath = 'TODO_PUBLISH_PATH'
$AppPath = 'TODO_APP_PATH'
$AppPool = 'TODO_APP_POOL'

Import-Module WebAdministration
Stop-WebAppPool -Name $AppPool
Copy-Item -Path (Join-Path $PublishPath '*') -Destination $AppPath -Recurse -Force
Start-WebAppPool -Name $AppPool
```

Windows service template:

```powershell
$ErrorActionPreference = 'Stop'
$PublishPath = 'TODO_PUBLISH_PATH'
$AppPath = 'TODO_APP_PATH'
$ServiceName = 'TODO_SERVICE_NAME'

Stop-Service -Name $ServiceName -ErrorAction Stop
Copy-Item -Path (Join-Path $PublishPath '*') -Destination $AppPath -Recurse -Force
Start-Service -Name $ServiceName -ErrorAction Stop
```

Linux/systemd template:

```bash
set -euo pipefail
publish_path="TODO_PUBLISH_PATH"
app_path="TODO_APP_PATH"
service_name="TODO_SERVICE_NAME"

sudo systemctl stop "$service_name"
sudo rsync -a --delete "$publish_path/" "$app_path/"
sudo systemctl start "$service_name"
```

## 7. Healthcheck And Smoke Test

Minimum checks:

```powershell
$BaseUrl = 'TODO_BASE_URL'
Invoke-WebRequest "$BaseUrl/health" -UseBasicParsing -TimeoutSec 15
Invoke-WebRequest "$BaseUrl/api/Auth/test" -UseBasicParsing -TimeoutSec 15
```

Authenticated smoke test:
- Login real admin/user.
- Confirm `POST /api/Auth/login` returns `200` and JWT.
- Confirm API calls use `Authorization: Bearer`.
- Confirm SignalR connects to `/dashboardHub`.
- Confirm mission reports endpoints work only after migration validation.
- Confirm logs show no startup DB errors.

## 8. Rollback

Rollback must be possible before deploy starts.

Windows/IIS rollback:

```powershell
$ErrorActionPreference = 'Stop'
$BackupPath = 'TODO_APP_BACKUP_PATH'
$AppPath = 'TODO_APP_PATH'
$AppPool = 'TODO_APP_POOL'

Import-Module WebAdministration
Stop-WebAppPool -Name $AppPool
Remove-Item -LiteralPath $AppPath -Recurse -Force
Copy-Item -LiteralPath $BackupPath -Destination $AppPath -Recurse -Force
Start-WebAppPool -Name $AppPool
```

DB rollback template:

```sql
ALTER DATABASE [TODO_DATABASE_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [TODO_DATABASE_NAME]
FROM DISK = N'TODO_BACKUP_FILE.bak'
WITH REPLACE, RECOVERY, CHECKSUM, STATS = 10;
ALTER DATABASE [TODO_DATABASE_NAME] SET MULTI_USER;
```

Post-rollback smoke:
- Health endpoint OK.
- Login OK.
- Existing dashboard pages load.
- Logs show no migration/startup errors.

## 9. Deploy Decision

Proceed only if:
- Backup app: verified.
- Backup DB: verified.
- Rollback: tested or rehearsed with exact commands.
- Secrets: configured outside repo.
- Migration: validated.
- Healthcheck: known and passing.
- Dashboard 1: no references.

If any item is missing, do not deploy.
