# Player Pace filter toggle — build + config key smoke
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

Write-Host '== Build WebApi ==' -ForegroundColor Cyan
dotnet build (Join-Path $root 'backend\WebApi\WebApi.csproj') -v q
if ($LASTEXITCODE -ne 0) { throw 'WebApi build failed' }

Write-Host '== Build frontend ==' -ForegroundColor Cyan
Push-Location (Join-Path $root 'frontend')
npm run build --silent
if ($LASTEXITCODE -ne 0) { Pop-Location; throw 'Frontend build failed' }
Pop-Location

$required = @(
    'PLAYER_PACE_FILTER_ENABLED',
    'api/player-pace-filter',
    'PlayerPaceFilterController',
    'getPlayerPaceFilter',
    'togglePlayerPaceFilter'
)
$bundle = @(
    (Join-Path $root 'backend\WebApi\Controllers\PlayerPaceFilterController.cs'),
    (Join-Path $root 'backend\WebApi\Services\Implementations\DashboardService.cs'),
    (Join-Path $root 'frontend\src\components\dashboard\StatsWidget.vue'),
    (Join-Path $root 'frontend\src\service\DashboardService.ts')
) | ForEach-Object { Get-Content -Raw -Path $_ } | Out-String

foreach ($needle in $required) {
    if ($bundle -notmatch [regex]::Escape($needle)) {
        throw "Missing '$needle' in player pace toggle patch"
    }
}

Write-Host 'PLAYER_PACE_FILTER_TOGGLE_SMOKE PASS' -ForegroundColor Green
