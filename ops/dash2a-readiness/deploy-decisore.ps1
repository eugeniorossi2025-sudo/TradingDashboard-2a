# deploy-decisore.ps1
# Idempotent deploy script for the Decisore engine (51.178.16.37).
# Compatible with Windows PowerShell 5.1
#
# Can be run:
#   - Manually via RDP on the Decisore VPS
#   - By the GitHub Actions workflow deploy-decisore.yml
#
# Usage:
#   .\deploy-decisore.ps1 -ArtifactPath C:\path\to\publish
#   .\deploy-decisore.ps1 -ArtifactPath C:\path\to\publish -SiteName decisore -AppPoolName decisore
#
# If -ArtifactPath is omitted, the script builds from the local repo checkout.
# Always runs the same steps - safe to re-run.

param(
    [string]$ArtifactPath       = '',
    [string]$SiteName           = 'default',
    [string]$AppPoolName        = 'Proactive',
    [string]$ReleaseRoot        = 'C:\inetpub\decisore\releases',
    [string]$SharedConfigPath   = 'C:\inetpub\decisore\shared\appsettings.Production.json',
    [string]$HealthUrl          = 'http://127.0.0.1/api/proactive/health',
    [int]   $HealthTimeoutSec   = 30,
    [int]   $StartupWaitSec     = 12,
    [string]$RepoRoot           = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-Step { param([string]$msg) Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok   { param([string]$msg) Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Warn { param([string]$msg) Write-Host "    WARN: $msg" -ForegroundColor Yellow }

# Resolve repo root
if (-not $RepoRoot) {
    $RepoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
}
$decisionEnginePath = Join-Path $RepoRoot 'decision-engine\Decisore'

# Step 1: Build + publish (if no artifact provided)
if (-not $ArtifactPath) {
    Write-Step "Building Decisore from: $decisionEnginePath"

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) { throw '.NET SDK not found. Install it on this machine.' }

    $ArtifactPath = Join-Path $env:TEMP ("decisore-publish-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    & dotnet publish "$decisionEnginePath\Decisore.csproj" `
        --configuration Release `
        --output $ArtifactPath `
        --no-self-contained
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
    Write-Ok "Published to $ArtifactPath"
} else {
    Write-Step "Using provided artifact: $ArtifactPath"
    if (-not (Test-Path $ArtifactPath)) { throw "ArtifactPath not found: $ArtifactPath" }
}

# Step 2: Detect runtime (IIS via appcmd.exe or Windows Service)
Write-Step 'Detecting Decisore runtime'

$useIIS      = $false
$useService  = $false
$serviceName = 'Decisore'
$appcmd      = "$env:SystemRoot\System32\inetsrv\appcmd.exe"

if (Test-Path $appcmd) {
    $poolList = & $appcmd list apppool $AppPoolName 2>&1
    if ($poolList -match $AppPoolName) {
        $useIIS = $true
        Write-Ok "Runtime: IIS app pool '$AppPoolName' found via appcmd.exe"
    }
}

if (-not $useIIS) {
    $svc = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($svc) {
        $useService = $true
        Write-Ok "Runtime: Windows Service '$serviceName' found"
    }
}

if (-not $useIIS -and -not $useService) {
    throw "Cannot find IIS app pool '$AppPoolName' or Windows Service '$serviceName'. Verify IIS/service setup."
}

# Step 3: Prepare release folder
$timestamp   = Get-Date -Format 'yyyyMMdd-HHmmss'
$releasePath = Join-Path $ReleaseRoot ("decisore-" + $timestamp)

Write-Step "Preparing release folder: $releasePath"
New-Item -ItemType Directory -Force -Path $releasePath | Out-Null
Get-ChildItem -LiteralPath $ArtifactPath -Force | Copy-Item -Destination $releasePath -Recurse -Force
$fileCount = (Get-ChildItem $releasePath -Recurse -File).Count
Write-Ok "$fileCount files copied"

# Step 4: Apply shared production config
Write-Step 'Applying production config'
if (Test-Path $SharedConfigPath) {
    Copy-Item -LiteralPath $SharedConfigPath `
        -Destination (Join-Path $releasePath 'appsettings.Production.json') -Force
    Write-Ok "appsettings.Production.json applied from $SharedConfigPath"
} else {
    Write-Warn "Shared config NOT found at $SharedConfigPath - using binaries config as-is"
}

# Step 5: Record current path for rollback (appcmd-based)
$previousPath = ''
if ($useIIS) {
    $vdirInfo = & $appcmd list vdir "$SiteName/" 2>&1
    if ($vdirInfo -match 'physicalPath:([^,\)]+)') {
        $previousPath = $Matches[1].Trim()
    }
    if (-not $previousPath) {
        $previousPath = "C:\Decisore"
        Write-Warn "Could not read previous path from appcmd - using fallback: $previousPath"
    }
} else {
    $svcObj = Get-WmiObject Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
    if ($svcObj) {
        $previousPath = Split-Path $svcObj.PathName -Parent
    }
}
Write-Ok "Previous path recorded: $previousPath"

# Deploy block with rollback on failure
try {
    # Step 6: Stop runtime
    if ($useIIS) {
        Write-Step "Stopping IIS app pool '$AppPoolName' via appcmd.exe"
        & $appcmd stop apppool /apppool.name:$AppPoolName | Out-Host
        $waited = 0
        do {
            Start-Sleep -Seconds 2
            $waited += 2
            $poolState = (& $appcmd list apppool $AppPoolName /text:state 2>&1)
        } while ($poolState -notmatch 'Stopped' -and $waited -lt 30)
        if ($poolState -notmatch 'Stopped') {
            throw "App pool did not stop within 30s (state: $poolState)"
        }
        Write-Ok "App pool stopped after ${waited}s"
    } else {
        Write-Step "Stopping Windows Service '$serviceName'"
        Stop-Service -Name $serviceName -Force
        Start-Sleep -Seconds 3
        Write-Ok 'Service stopped'
    }

    # Step 7: Swap to new release via appcmd.exe
    Write-Step "Swapping to new release: $releasePath"
    if ($useIIS) {
        & $appcmd set vdir "$SiteName/" /physicalPath:$releasePath | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "appcmd set vdir failed (exit $LASTEXITCODE)" }
    } else {
        $exePath = Join-Path $releasePath 'Decisore.exe'
        if (Test-Path $exePath) {
            Set-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Services\$serviceName" `
                -Name ImagePath -Value $exePath
        } else {
            Write-Warn 'Decisore.exe not found in release - service path not updated'
        }
    }
    Write-Ok 'Swap complete'

    # Step 8: Start runtime
    if ($useIIS) {
        Write-Step "Starting IIS app pool '$AppPoolName' via appcmd.exe"
        & $appcmd start apppool /apppool.name:$AppPoolName | Out-Host
    } else {
        Write-Step "Starting Windows Service '$serviceName'"
        Start-Service -Name $serviceName
    }
    Write-Ok "Runtime started - waiting ${StartupWaitSec}s for process warm-up"
    Start-Sleep -Seconds $StartupWaitSec

    # Step 9: Health check - GET /api/proactive/health, no side-effects
    Write-Step "Health check: $HealthUrl"
    $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing `
        -TimeoutSec $HealthTimeoutSec -MaximumRedirection 0 -ErrorAction Stop

    if ($response.StatusCode -ne 200) {
        throw "Health check returned HTTP $($response.StatusCode)"
    }

    $json = $response.Content | ConvertFrom-Json
    if ($json.status -ne 'ok') {
        throw "Health check status unexpected: '$($json.status)' (expected 'ok')"
    }
    Write-Ok "Health check OK (HTTP 200, status=$($json.status), service=$($json.service))"

} catch {
    Write-Host ''
    Write-Host "DEPLOY FAILED: $($_.Exception.Message)" -ForegroundColor Red

    if ($previousPath -and (Test-Path $previousPath)) {
        Write-Step "ROLLBACK: restoring to $previousPath"
        if ($useIIS) {
            & $appcmd set vdir "$SiteName/" /physicalPath:$previousPath | Out-Host
            & $appcmd start apppool /apppool.name:$AppPoolName | Out-Host
        } else {
            $oldExe = Join-Path $previousPath 'Decisore.exe'
            if (Test-Path $oldExe) {
                Set-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Services\$serviceName" `
                    -Name ImagePath -Value $oldExe
            }
            Start-Service -Name $serviceName -ErrorAction SilentlyContinue
        }
        Write-Host "Rollback completato - runtime ripristinato su: $previousPath" -ForegroundColor Yellow
    } else {
        Write-Warn 'No valid previous path for rollback - manual intervention required'
    }
    throw
}

Write-Host ''
Write-Host 'DEPLOY OK' -ForegroundColor Green
Write-Host "RELEASE_PATH=$releasePath"
Write-Host "PREVIOUS_PATH=$previousPath"
Write-Host "HEALTH_URL=$HealthUrl"
Write-Host "TIMESTAMP=$(Get-Date -Format 'o')"
