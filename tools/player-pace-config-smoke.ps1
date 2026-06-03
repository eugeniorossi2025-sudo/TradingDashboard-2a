# PLAYER pace threshold + streak smoke (local)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

Write-Host "== Build Decisore ==" -ForegroundColor Cyan
dotnet build (Join-Path $root "decision-engine\Decisore\Decisore.csproj") -v q
if ($LASTEXITCODE -ne 0) { throw "Decisore build failed" }

Write-Host "== C# config/streak smoke ==" -ForegroundColor Cyan
dotnet run --project (Join-Path $root "tools\player-pace-config-smoke\PlayerPaceConfigSmoke.csproj") -v q
if ($LASTEXITCODE -ne 0) { throw "C# smoke failed" }

Write-Host "== JS threshold smoke ==" -ForegroundColor Cyan
node (Join-Path $root "tools\player-pace-config-smoke\threshold-ui-smoke.mjs")
if ($LASTEXITCODE -ne 0) { throw "JS smoke failed" }

Write-Host "== Build frontend ==" -ForegroundColor Cyan
Push-Location (Join-Path $root "frontend")
npm run build --silent
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Frontend build failed" }
Pop-Location

Write-Host "PLAYER_PACE_LOCAL_SMOKE PASS" -ForegroundColor Green
