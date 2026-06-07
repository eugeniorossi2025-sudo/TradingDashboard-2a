# Pre-deploy: HTML hero must equal periodResultEuro; MissionMarginEuro must equal MissionSessions.TotalMargin (DB).
# Usage:
#   $env:DASH2A_SQL = 'Server=...;Database=...;...'
#   .\ops\dash2a-readiness\verify-mission-margin-predeploy.ps1
# Or after deploy (HTML on server):
#   .\ops\dash2a-readiness\verify-mission-margin-predeploy.ps1 -Api https://vps-b0942869.vps.ovh.net -HtmlOnly

param(
    [string]$Api = '',
    [string]$User = 'admin',
    [string]$Pass = 'Admin@123456',
    [switch]$HtmlOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$expectedClosingMargin = @{
    102 = 39.60
    103 = 99.20
    104 = 125.10
    105 = 254.40
}

function Unwrap-Token($login) {
    $payload = if ($login.data) { $login.data } else { $login }
    if ($payload.token) { return $payload.token }
    if ($payload.Token) { return $payload.Token }
    throw 'Login failed: token missing'
}

function Parse-HeroPeriodResult([string]$html) {
    if ($html -match 'RISULTATO PERIODO[^<]*</div>\s*<div class="heroValue[^"]*">([+-]?[\d.,]+)') {
        $raw = $Matches[1].Replace(',', '.')
        return [decimal]::Parse($raw, [System.Globalization.CultureInfo]::InvariantCulture)
    }
    return $null
}

if (-not $HtmlOnly) {
    $conn = $env:DASH2A_SQL
    if ([string]::IsNullOrWhiteSpace($conn)) {
        Write-Host 'DASH2A_SQL not set — running dotnet verify tool requires SQL connection.'
        Write-Host 'Set DASH2A_SQL or pass -HtmlOnly after deploy.'
        exit 4
    }

    Write-Host '=== DB + builder + HTML verify (local code) ==='
    Push-Location $repoRoot
    try {
        & dotnet run --project tools/MissionMarginPredeployVerify/MissionMarginPredeployVerify.csproj --no-build -p:UseAppHost=false -- --connection $conn
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    finally {
        Pop-Location
    }
}

if ([string]::IsNullOrWhiteSpace($Api)) {
    if (-not $HtmlOnly) {
        Write-Host 'ALL PASS (local DB verify). Skipping remote HTML (no -Api).'
        exit 0
    }
    throw '-Api required for -HtmlOnly'
}

Write-Host "=== Remote HTML verify ($Api) ==="
$login = Invoke-RestMethod -Uri "$Api/api/Auth/login" -Method POST -ContentType 'application/json' -Body (@{ username = $User; password = $Pass } | ConvertTo-Json)
$token = Unwrap-Token $login
$h = @{ Authorization = "Bearer $token" }

$fail = 0
foreach ($id in ($expectedClosingMargin.Keys | Sort-Object)) {
    $json = Invoke-RestMethod -Uri "$Api/api/mission/report/$id`?format=json" -Headers $h
    $report = if ($json.data) { $json.data } else { $json }
    $expectedPeriod = [decimal]$report.totals.periodResultEuro
    $session = @($report.sessions | Where-Object { $_.sessionId -eq $id } | Select-Object -First 1)
    $closing = if ($session) { [decimal]$session.missionMarginEuro } else { [decimal]0 }

    $html = (Invoke-WebRequest -Uri "$Api/api/mission/report/$id`?format=html" -Headers $h -UseBasicParsing).Content
    $hero = Parse-HeroPeriodResult $html
    $hasMarker = $html -match 'mission-report-html:v2026-06-07-period-result-hero'
    $heroOk = $hero.HasValue -and [Math]::Abs($hero.Value - $expectedPeriod) -lt 0.01m
    $closingOk = $expectedClosingMargin.ContainsKey($id) -and [Math]::Abs($closing - [decimal]$expectedClosingMargin[$id]) -lt 0.01m
    $ok = $heroOk -and $closingOk -and $hasMarker
    if (-not $ok) { $fail++ }
    $status = if ($ok) { 'PASS' } else { 'FAIL' }
    Write-Host "$status #$id hero=$hero periodResult=$expectedPeriod closing=$closing marker=$hasMarker"
}

if ($fail -gt 0) { exit 3 }
Write-Host 'HTML spot-check ALL PASS'
exit 0
