# Validates canonical report accounting: Header = Σ missions = Σ daily = last curve point
# Usage (post-deploy): .\ops\dash2a-readiness\validate-report-coherence.ps1 -From 2026-05-01 -To 2026-05-30
param(
    [string]$Api = 'https://vps-b0942869.vps.ovh.net',
    [string]$User = 'admin',
    [string]$Pass = 'Admin@123456',
    [string]$From = '2026-05-01',
    [string]$To = '2026-05-30',
    [string]$RuntimeMode = 'Production'
)

$ErrorActionPreference = 'Stop'
$login = Invoke-RestMethod -Uri "$Api/api/Auth/login" -Method POST -ContentType 'application/json' -Body (@{ username = $User; password = $Pass } | ConvertTo-Json)
$loginPayload = if ($login.data) { $login.data } else { $login }
$token = $loginPayload.token
$h = @{ Authorization = "Bearer $token" }

$r = Invoke-RestMethod -Uri "$Api/api/mission/report/range?runtimeMode=$RuntimeMode&from=$From&to=$To&format=json&summary=false" -Headers $h
$d = if ($r.data) { $r.data } else { $r }

$period = [decimal]$d.totals.periodResultEuro
$legacy = [decimal]$d.totals.totalMarginEuro
$sessionSum = [decimal](($d.sessions | Measure-Object totalMarginEuro -Sum).Sum)
$dailySum = [decimal](($d.dailyRows | Measure-Object netPnl -Sum).Sum)
$lastCurve = if ($d.dailyRows.Count -gt 0) { [decimal]$d.dailyRows[-1].cumulativePnl } else { [decimal]0 }

$checks = @(
    @{ id = 'ACC-01'; name = 'periodResultEuro present'; pass = ($null -ne $d.totals.periodResultEuro); detail = "periodResult=$period" }
    @{ id = 'ACC-02'; name = 'periodResultEuro = totalMarginEuro (compat)'; pass = ($period -eq $legacy); detail = "legacy=$legacy" }
    @{ id = 'ACC-03'; name = 'Header = Σ missioni'; pass = ($period -eq $sessionSum); detail = "sessionSum=$sessionSum" }
    @{ id = 'ACC-04'; name = 'Header = Σ daily'; pass = ($period -eq $dailySum); detail = "dailySum=$dailySum" }
    @{ id = 'ACC-05'; name = 'Header = ultimo punto curva'; pass = ($period -eq $lastCurve); detail = "lastCurve=$lastCurve" }
)

Write-Host "=== Report accounting coherence ($From -> $To $RuntimeMode) ==="
$fail = 0
foreach ($c in $checks) {
    $status = if ($c.pass) { 'PASS' } else { 'FAIL'; $fail++ }
    Write-Host "$status $($c.id) $($c.name) :: $($c.detail)"
}

if ($fail -gt 0) { exit 2 }
Write-Host "ALL PASS ($($checks.Count)/$($checks.Count))"
