# DASH2A GitHub Actions self-hosted runner setup.
# Run manually on the VPS in an elevated PowerShell session.
# This installs only the GitHub Actions runner service. It does not deploy, restart IIS, restart the server, or change firewall rules.

$ErrorActionPreference = 'Stop'

$RepoUrl = 'https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a'
$RunnerName = 'dash2a-windows-runner-01'
$RunnerRoot = 'C:\actions-runner'
$RunnerDir = Join-Path $RunnerRoot $RunnerName
$RunnerLabels = 'DASH2A'

Write-Host 'DASH2A GitHub Actions self-hosted runner setup'
Write-Host 'No deploy. No IIS restart. No server restart. No firewall changes.'
Write-Host 'Token will be requested interactively and will not be printed.'

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).
  IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
  throw 'Run this script from PowerShell as Administrator.'
}

if (Test-Path (Join-Path $RunnerDir '.runner')) {
  $existingService = Get-CimInstance Win32_Service | Where-Object {
    $_.Name -like 'actions.runner.*' -and $_.PathName -like "*$RunnerDir*"
  } | Select-Object -First 1
  if (-not $existingService) { throw "Runner configured but Windows service missing for $RunnerDir" }
  if ($existingService.State -ne 'Running') { Start-Service -Name $existingService.Name }
  if ((Get-Service -Name $existingService.Name).Status -ne 'Running') {
    throw "Runner service is not running: $($existingService.Name)"
  }
  Write-Host "Runner already configured and service running: $($existingService.Name)." -ForegroundColor Green
  exit 0
}

$runnerToken = Read-Host 'Paste a NEW GitHub runner registration token'
if ([string]::IsNullOrWhiteSpace($runnerToken)) {
  throw 'Runner registration token is required.'
}

try {
  New-Item -ItemType Directory -Force -Path $RunnerDir | Out-Null
  Set-Location $RunnerDir

  $release = Invoke-RestMethod `
    -Uri 'https://api.github.com/repos/actions/runner/releases/latest' `
    -Headers @{ 'User-Agent' = 'dash2a-runner-setup' }

  $asset = $release.assets |
    Where-Object { $_.name -match '^actions-runner-win-x64-.*\.zip$' } |
    Select-Object -First 1

  if (-not $asset) {
    throw 'Could not find latest Windows x64 runner asset.'
  }

  $zip = Join-Path $RunnerDir $asset.name
  Write-Host "Downloading runner package: $($asset.name)"
  Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zip

  Write-Host 'Extracting runner package...'
  Expand-Archive -Path $zip -DestinationPath $RunnerDir -Force

  Write-Host 'Configuring runner as Windows Service...'
  & .\config.cmd `
    --unattended `
    --url $RepoUrl `
    --token $runnerToken `
    --name $RunnerName `
    --labels $RunnerLabels `
    --work '_work' `
    --runasservice

  if ($LASTEXITCODE -ne 0) {
    throw "config.cmd failed with exit code $LASTEXITCODE"
  }

  Write-Host 'Checking Windows runner service...'
  $service = Get-CimInstance Win32_Service | Where-Object {
    $_.Name -like 'actions.runner.*' -and $_.PathName -like "*$RunnerDir*"
  } | Select-Object -First 1
  if (-not $service) {
    throw "Runner service was not registered for $RunnerDir"
  }
  if ($service.State -ne 'Running') {
    Start-Service -Name $service.Name
  }
  $service = Get-Service -Name $service.Name
  if ($service.Status -ne 'Running') {
    throw "Runner service is not running: $($service.Name)"
  }

  Write-Host "Runner service running: $($service.Name). Verify GitHub shows online/idle with labels: self-hosted, Windows, DASH2A." -ForegroundColor Green
}
finally {
  $runnerToken = $null
}
