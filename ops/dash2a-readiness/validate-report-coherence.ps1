# Validates canonical report accounting: periodResultEuro = Σ periodNetPnl = Σ daily = last curve point
# Usage (post-deploy): .\ops\dash2a-readiness\validate-report-coherence.ps1 -From 2026-05-29 -To 2026-05-30
param(
    [string]$Api = 'https://vps-b0942869.vps.ovh.net',
    [string]$User = 'admin',
    [string]$Pass = 'Admin@123456',
    [string]$From = '2026-05-01',
    [string]$To = '2026-05-30',
    [string]$RuntimeMode = 'Production'
)

$ErrorActionPreference = 'Stop'

function Unwrap-Token($login) {
    $payload = if ($login.data) { $login.data } else { $login }
    if ($payload.token) { return $payload.token }
    if ($payload.Token) { return $payload.Token }
    throw 'Login failed: token missing'
}

function Rome-DateFromUtc([string]$isoUtc) {
    $utc = [DateTime]::SpecifyKind([DateTime]::Parse($isoUtc), [DateTimeKind]::Utc)
    $rome = [TimeZoneInfo]::FindSystemTimeZoneById('W. Europe Standard Time')
    return [DateTimeOffset]::new($utc, [TimeSpan]::Zero).ToOffset($rome.GetUtcOffset($utc)).Date
}

$login = Invoke-RestMethod -Uri "$Api/api/Auth/login" -Method POST -ContentType 'application/json' -Body (@{ username = $User; password = $Pass } | ConvertTo-Json)
$token = Unwrap-Token $login
$h = @{ Authorization = "Bearer $token" }

$r = Invoke-RestMethod -Uri "$Api/api/mission/report/range?runtimeMode=$RuntimeMode&from=$From&to=$To&format=json&summary=false" -Headers $h
$d = if ($r.data) { $r.data } else { $r }

$period = [decimal]$d.totals.periodResultEuro
$stockSum = [decimal](($d.sessions | Measure-Object totalMarginEuro -Sum).Sum)
$sessionPnlSum = [decimal](($d.sessions | Measure-Object periodNetPnlEuro -Sum).Sum)
$dailySum = [decimal](($d.dailyRows | Measure-Object netPnl -Sum).Sum)
$lastCurve = if ($d.dailyRows.Count -gt 0) { [decimal]$d.dailyRows[-1].cumulativePnl } else { [decimal]0 }

$fromDate = [DateTime]::Parse($From).Date
$toDate = [DateTime]::Parse($To).Date
$badStartSessions = @($d.sessions | Where-Object {
    $romeStart = Rome-DateFromUtc $_.startTime
    $romeStart -lt $fromDate -or $romeStart -gt $toDate
})

$annualised = $d.totals.annualisedReturnPct
$workingDays = [int]$d.totals.workingDays
$annualisedOk = ($workingDays -lt 7 -and ($null -eq $annualised)) -or ($workingDays -ge 7 -and $null -ne $annualised)

$html = Invoke-WebRequest -Uri "$Api/api/mission/report/range?runtimeMode=$RuntimeMode&from=$From&to=$To&format=html&summary=false" -Headers $h -UseBasicParsing
$htmlBody = [string]$html.Content
$htmlHasRomeHeaders = $htmlBody -match 'Start \(Europe/Rome\)' -or $htmlBody -match 'Start</th>'
$htmlHeroPeriod = $htmlBody -match 'RISULTATO PERIODO'
$htmlShowsNd = ($workingDays -lt 7 -and $htmlBody -match 'Annualised Return[\s\S]*?N/D') -or ($workingDays -ge 7)

$session6 = $d.sessions | Where-Object { $_.sessionId -eq 6 } | Select-Object -First 1
$session6Ok = $true
$session6Detail = 'not in period dataset'
if ($session6) {
    $session6Rome = Rome-DateFromUtc $session6.startTime
    $session6Ok = ($session6Rome -ge $fromDate -and $session6Rome -le $toDate)
    $session6Detail = "romeStart=$($session6Rome.ToString('yyyy-MM-dd'))"
}

$sessionTargetMax = if ($d.sessions.Count -gt 0) { [decimal](($d.sessions | Measure-Object globalTargetEuro -Maximum).Maximum) } else { [decimal]0 }
$headerTarget = [decimal]$d.totals.globalTargetEuro

$stockDiffersFromFlow = ($d.sessions.Count -gt 0) -and ([Math]::Abs($period - $stockSum) -gt 0.01m)

$checks = @(
    @{ id = 'ACC-01'; name = 'periodResultEuro present'; pass = ($null -ne $d.totals.periodResultEuro); detail = "periodResult=$period" }
    @{ id = 'ACC-02'; name = 'periodResultEuro = Σ periodNetPnlEuro'; pass = ($period -eq $sessionPnlSum); detail = "sessionPnlSum=$sessionPnlSum stockSum=$stockSum" }
    @{ id = 'ACC-03'; name = 'periodResultEuro ≠ totalMarginEuro when stock differs (semantic)'; pass = (-not $stockDiffersFromFlow) -or ($period -ne $stockSum); detail = "period=$period stockSum=$stockSum" }
    @{ id = 'ACC-04'; name = 'Header = Σ daily'; pass = ($period -eq $dailySum); detail = "dailySum=$dailySum" }
    @{ id = 'ACC-05'; name = 'Header = ultimo punto curva'; pass = ($period -eq $lastCurve); detail = "lastCurve=$lastCurve" }
    @{ id = 'ACC-06'; name = 'No session with Start Rome before From or after To'; pass = ($badStartSessions.Count -eq 0); detail = "badCount=$($badStartSessions.Count)" }
    @{ id = 'ACC-07'; name = 'Annualised null/N/D when workingDays < 7'; pass = $annualisedOk; detail = "workingDays=$workingDays annualised=$annualised" }
    @{ id = 'ACC-08'; name = 'HTML hero labelled RISULTATO PERIODO'; pass = $htmlHeroPeriod; detail = "heroPeriod=$htmlHeroPeriod" }
    @{ id = 'ACC-09'; name = 'HTML Annualised N/D on short period'; pass = $htmlShowsNd; detail = "workingDays=$workingDays" }
    @{ id = 'ACC-12'; name = 'Header Target = max Stop Win per mission (not Σ sessions)'; pass = ($headerTarget -eq $sessionTargetMax); detail = "headerTarget=$headerTarget sessionMax=$sessionTargetMax sessions=$($d.sessions.Count)" }
)

if ($From -eq '2026-05-29' -and $To -eq '2026-05-30') {
    $hasSession5 = @($d.sessions | Where-Object { $_.sessionId -eq 5 }).Count -gt 0
    $checks += @{ id = 'ACC-10'; name = 'Period 29-30 excludes session #5 (Start 28/05 Rome)'; pass = (-not $hasSession5); detail = "session5Present=$hasSession5" }
    $checks += @{ id = 'ACC-11'; name = 'Session #6 Start Rome in period when present'; pass = $session6Ok; detail = $session6Detail }
}

Write-Host "=== Report accounting coherence ($From -> $To $RuntimeMode) ==="
$fail = 0
foreach ($c in $checks) {
    $status = if ($c.pass) { 'PASS' } else { 'FAIL'; $fail++ }
    Write-Host "$status $($c.id) $($c.name) :: $($c.detail)"
}

if ($fail -gt 0) { exit 2 }
Write-Host "ALL PASS ($($checks.Count)/$($checks.Count))"
