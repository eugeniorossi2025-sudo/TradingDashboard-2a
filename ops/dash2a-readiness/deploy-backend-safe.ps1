param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactPath,

    [string]$SiteName = 'demoapp',
    [string]$AppPoolName = 'demoapp',
    [string]$CurrentPath = 'C:\inetpub\wwwroot\publish',
    [string]$ReleaseRoot = 'C:\inetpub\wwwroot\releases',
    [string]$BackupRoot = 'C:\inetpub\wwwroot\backups',
    [string]$SmokeTestUrl = 'http://localhost/api/Auth/test'
)

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message"
}

function Assert-Path {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Description not found: $Path"
    }
}

function Copy-DirectoryContents {
    param(
        [string]$Source,
        [string]$Destination
    )

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | Copy-Item -Destination $Destination -Recurse -Force
}

Write-Step 'Validating deploy inputs'
Assert-Path -Path $ArtifactPath -Description 'Published artifact path'
Assert-Path -Path $CurrentPath -Description 'Current IIS publish path'

$forbiddenDbTools = @(
    'dotnet-ef.exe',
    'ef.exe'
)

foreach ($tool in $forbiddenDbTools) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) {
        Write-Host "DB migration tool present but not used: $tool"
    }
}

Import-Module WebAdministration

$site = Get-Website -Name $SiteName -ErrorAction Stop
$appPool = Get-Item "IIS:\AppPools\$AppPoolName" -ErrorAction Stop

Write-Step "IIS target confirmed: site=$($site.Name), appPool=$($appPool.Name), currentPath=$CurrentPath"

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$releasePath = Join-Path $ReleaseRoot "backend-$timestamp"
$backupPath = Join-Path $BackupRoot "publish-$timestamp"

Write-Step "Creating release path: $releasePath"
New-Item -ItemType Directory -Force -Path $releasePath | Out-Null
Copy-DirectoryContents -Source $ArtifactPath -Destination $releasePath

Write-Step "Backing up current publish path: $backupPath"
New-Item -ItemType Directory -Force -Path $backupPath | Out-Null
Copy-DirectoryContents -Source $CurrentPath -Destination $backupPath

$appOffline = Join-Path $CurrentPath 'app_offline.htm'
$previousPath = "$CurrentPath.previous-$timestamp"

try {
    Write-Step 'Stopping app pool to release file locks'
    Stop-WebAppPool -Name $AppPoolName
    $waited = 0
    do {
        Start-Sleep -Seconds 2
        $waited += 2
        $state = (Get-WebAppPoolState -Name $AppPoolName).Value
    } while ($state -ne 'Stopped' -and $waited -lt 30)
    if ($state -ne 'Stopped') { throw "App pool did not stop within 30s (state: $state)" }
    Write-Host "App pool stopped after ${waited}s"

    Write-Step 'Swapping publish directory'
    Rename-Item -LiteralPath $CurrentPath -NewName (Split-Path $previousPath -Leaf)
    New-Item -ItemType Directory -Force -Path $CurrentPath | Out-Null
    Copy-DirectoryContents -Source $releasePath -Destination $CurrentPath

    Write-Step "Starting app pool: $AppPoolName"
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 10

    Write-Step "Smoke test: $SmokeTestUrl"
    $response = Invoke-WebRequest -Uri $SmokeTestUrl -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -ne 200) {
        throw "Smoke test failed with HTTP $($response.StatusCode)"
    }

    Write-Step 'Smoke test passed'
    Write-Host "RELEASE_PATH=$releasePath"
    Write-Host "BACKUP_PATH=$backupPath"
    Write-Host "PREVIOUS_PATH=$previousPath"
    Write-Host "IIS_SITE=$SiteName"
    Write-Host "IIS_APP_POOL=$AppPoolName"
    Write-Host "SMOKE_STATUS=$($response.StatusCode)"
}
catch {
    Write-Host "DEPLOY_FAILED=$($_.Exception.Message)"
    Write-Step 'Attempting rollback to previous publish path'

    if (Test-Path -LiteralPath $CurrentPath) {
        Remove-Item -LiteralPath $CurrentPath -Recurse -Force
    }

    if (Test-Path -LiteralPath $previousPath) {
        Rename-Item -LiteralPath $previousPath -NewName (Split-Path $CurrentPath -Leaf)
    }
    elseif (Test-Path -LiteralPath $backupPath) {
        New-Item -ItemType Directory -Force -Path $CurrentPath | Out-Null
        Copy-DirectoryContents -Source $backupPath -Destination $CurrentPath
    }

    Start-WebAppPool -Name $AppPoolName
    throw
}
