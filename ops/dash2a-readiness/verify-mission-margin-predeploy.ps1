# Pre-deploy: MissionMarginEuro (builder+HTML) must equal MissionSessions.TotalMargin (DB).
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
$expected = @{
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

function Parse-HeroMargin([string]$html) {
    if ($html -match 'MARGINE MISSIONE[^<]*</div>\s*<div class="heroValue[^"]*">([+-]?[\d.,]+)\s*€') {
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
foreach ($id in ($expected.Keys | Sort-Object)) {
    $html = (Invoke-WebRequest -Uri "$Api/api/mission/report/$id`?format=html" -Headers $h -UseBasicParsing).Content
    $hero = Parse-HeroMargin $html
    $exp = [decimal]$expected[$id]
    $hasMarker = $html -match 'mission-report-html:v2026-06-04-db-total-margin'
    $ok = $hero.HasValue -and [Math]::Abs($hero.Value - $exp) -lt 0.01m
    if (-not $ok) { $fail++ }
    $status = if ($ok) { 'PASS' } else { 'FAIL' }
    Write-Host "$status #$id HTML hero=$($hero) expected=$exp marker=$hasMarker"
}

if ($fail -gt 0) { exit 3 }
Write-Host 'HTML spot-check ALL PASS'
exit 0
