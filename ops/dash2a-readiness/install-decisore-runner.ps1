# install-decisore-runner.ps1
# One-time setup of the GitHub Actions self-hosted runner on the Decisore VPS.
# Run once via RDP on 51.178.16.37 in an elevated PowerShell session.
#
# After this script completes, every push to main that touches decision-engine/**
# will automatically build and deploy the Decisore — no manual steps needed.
#
# This script ONLY installs the runner service.
# It does NOT deploy, restart IIS, restart the server, or change firewall rules.

$ErrorActionPreference = 'Stop'

$RepoUrl      = 'https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a'
$RunnerName   = 'dash2a-decisore-runner-01'
$RunnerRoot   = 'C:\actions-runner'
$RunnerDir    = Join-Path $RunnerRoot $RunnerName
$RunnerLabels = 'DASH2A,DASH2A-DECISORE'

Write-Host '============================================================'
Write-Host ' DASH2A Decisore — GitHub Actions self-hosted runner setup'
Write-Host ' Host: 51.178.16.37 (vps-4ca306e8.vps.ovh.net)'
Write-Host '============================================================'
Write-Host 'This script installs ONLY the runner service.'
Write-Host 'No deploy. No IIS restart. No server restart. No firewall changes.'
Write-Host ''

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).
    IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw 'Run from PowerShell as Administrator.' }

if (Test-Path (Join-Path $RunnerDir '.runner')) {
    Write-Host "Runner already configured in $RunnerDir. No changes made." -ForegroundColor Yellow
    exit 0
}

# --- Token ---
Write-Host ''
Write-Host 'To get a registration token:'
Write-Host "  1. Go to: https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a/settings/actions/runners/new?runnerOs=windows"
Write-Host '  2. Copy the token shown in the "Configure" step.'
Write-Host ''
$runnerToken = Read-Host 'Paste a NEW GitHub runner registration token'
if ([string]::IsNullOrWhiteSpace($runnerToken)) { throw 'Token is required.' }

# --- Download latest runner ---
New-Item -ItemType Directory -Force -Path $RunnerDir | Out-Null
Set-Location $RunnerDir

Write-Host ''
Write-Host 'Fetching latest runner release from GitHub...'
$release = Invoke-RestMethod `
    -Uri 'https://api.github.com/repos/actions/runner/releases/latest' `
    -Headers @{ 'User-Agent' = 'decisore-runner-setup' }

$asset = $release.assets |
    Where-Object { $_.name -match '^actions-runner-win-x64-.*\.zip$' } |
    Select-Object -First 1

if (-not $asset) { throw 'Could not find latest Windows x64 runner asset.' }

$zip = Join-Path $RunnerDir $asset.name
Write-Host "Downloading: $($asset.name)"
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zip
Write-Host 'Extracting...'
Expand-Archive -Path $zip -DestinationPath $RunnerDir -Force
Remove-Item $zip -Force

# --- Configure runner ---
Write-Host ''
Write-Host "Configuring runner '$RunnerName' with labels: $RunnerLabels"
& .\config.cmd `
    --unattended `
    --url $RepoUrl `
    --token $runnerToken `
    --name $RunnerName `
    --labels $RunnerLabels `
    --work '_work' `
    --runasservice `
    --windowslogonaccount 'NT AUTHORITY\NETWORK SERVICE'

if ($LASTEXITCODE -ne 0) { throw "Runner configuration failed (exit $LASTEXITCODE)" }

# --- Start service ---
Write-Host ''
Write-Host "Starting runner service: $RunnerName"
$svcName = "actions.runner.eugeniorossi2025--sudo-TradingDashboard-2a.$RunnerName"
$svc = Get-Service -Name $svcName -ErrorAction SilentlyContinue
if ($svc) {
    Start-Service -Name $svcName
    Write-Host "Service '$svcName' started." -ForegroundColor Green
} else {
    Write-Host "Service not found by guessed name — check Services.msc manually." -ForegroundColor Yellow
    & .\run.cmd --check
}

# --- Verify .NET SDK ---
Write-Host ''
Write-Host 'Checking .NET SDK availability...'
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnet) {
    Write-Host ".NET SDK: $( & dotnet --version )" -ForegroundColor Green
} else {
    Write-Host ''
    Write-Host 'WARNING: dotnet not found in PATH.' -ForegroundColor Yellow
    Write-Host 'Install .NET 10 SDK before running deploy workflows:'
    Write-Host '  https://dotnet.microsoft.com/download/dotnet/10.0'
    Write-Host 'After installing, restart the runner service.'
}

Write-Host ''
Write-Host '============================================================'
Write-Host " Runner '$RunnerName' configured and running." -ForegroundColor Green
Write-Host " Labels: $RunnerLabels"
Write-Host ' Verify at: https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a/settings/actions/runners'
Write-Host '============================================================'
Write-Host ''
Write-Host 'NEXT STEPS:'
Write-Host '  1. Verify runner appears online at the URL above.'
Write-Host '  2. Create IIS site "decisore" (port 80) if not existing:'
Write-Host '     New-Website -Name decisore -Port 80 -PhysicalPath C:\inetpub\decisore\current'
Write-Host '     New-WebAppPool -Name decisore'
Write-Host '     Set-ItemProperty IIS:\Sites\decisore -Name applicationPool -Value decisore'
Write-Host '  3. Create shared config folder:'
Write-Host '     New-Item -ItemType Directory -Force C:\inetpub\decisore\shared'
Write-Host '     New-Item -ItemType Directory -Force C:\inetpub\decisore\releases'
Write-Host '     New-Item -ItemType Directory -Force C:\inetpub\decisore\current'
Write-Host '     Copy the production appsettings.Production.json to:'
Write-Host '     C:\inetpub\decisore\shared\appsettings.Production.json'
Write-Host '  4. Push any change to decision-engine/** on main to trigger first auto-deploy.'
