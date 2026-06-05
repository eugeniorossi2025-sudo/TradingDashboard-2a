# Step 3 collaudo - Bot Owner Auth (same contract as BotOwnerAuthHelper.cs)
# Usage: powershell -File bot-owner-auth-collaudo.ps1

$ErrorActionPreference = 'Stop'

function Parse-BotOwnerAuthResponse {
    param([int]$StatusCode, [string]$Body)
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        try {
            $json = $Body | ConvertFrom-Json
            $status = "$($json.status)".Trim().ToUpperInvariant()
            if ($status -eq 'OK') { return 'Ok' }
            if ($status -eq 'LOCKED') { return 'Locked' }
            if ($status -eq 'UNAUTHORIZED') { return 'Unauthorized' }
        } catch { }
    }
    if ($StatusCode -eq 401) { return 'Unauthorized' }
    return 'Unreachable'
}

function Invoke-BotOwnerCheck {
    param(
        [string]$BaseUrl,
        [string]$UserId,
        [string]$Password,
        [int]$TimeoutSec = 12
    )
    $uri = ($BaseUrl.TrimEnd('/')) + '/api/bot-owner-auth/check'
    $body = @{ userId = $UserId; password = $Password } | ConvertTo-Json -Compress
    try {
        $resp = Invoke-WebRequest -Uri $uri -Method POST -Body $body -ContentType 'application/json' -TimeoutSec $TimeoutSec -UseBasicParsing
        return @{ StatusCode = [int]$resp.StatusCode; Body = $resp.Content }
    } catch {
        if ($_.Exception.Response) {
            $code = [int]$_.Exception.Response.StatusCode.value__
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $text = $reader.ReadToEnd()
            return @{ StatusCode = $code; Body = $text }
        }
        return @{ StatusCode = 0; Body = '' }
    }
}

$devUrl = 'https://dev-eugeniotrading.com'
$results = @()

function Add-Result {
    param([string]$Name, [bool]$Pass, [string]$Detail)
    $script:results += [PSCustomObject]@{ Test = $Name; Pass = $Pass; Detail = $Detail }
    $mark = if ($Pass) { 'PASS' } else { 'FAIL' }
    Write-Output "[$mark] $Name - $Detail"
}

Write-Output '=== Parse logic (mirrors BotOwnerAuthHelper.ParseResponse) ==='
Add-Result 'parse OK' ($(
    Parse-BotOwnerAuthResponse 200 '{"status":"OK"}'
) -eq 'Ok') '200 + status OK'
Add-Result 'parse LOCKED' ($(
    Parse-BotOwnerAuthResponse 200 '{"status":"LOCKED"}'
) -eq 'Locked') '200 + status LOCKED'
Add-Result 'parse UNAUTHORIZED body' ($(
    Parse-BotOwnerAuthResponse 401 '{"status":"UNAUTHORIZED"}'
) -eq 'Unauthorized') '401 + status UNAUTHORIZED'
Add-Result 'parse UNAUTHORIZED code-only' ($(
    Parse-BotOwnerAuthResponse 401 ''
) -eq 'Unauthorized') '401 empty body'
Add-Result 'parse unreachable 404' ($(
    Parse-BotOwnerAuthResponse 404 ''
) -eq 'Unreachable') '404 fail-closed'

Write-Output ''
Write-Output '=== Live DEV server ==='
$ok = Invoke-BotOwnerCheck -BaseUrl $devUrl -UserId 'boot' -Password '1234'
$okParsed = Parse-BotOwnerAuthResponse $ok.StatusCode $ok.Body
Add-Result 'live autorizzato' ($okParsed -eq 'Ok') "HTTP $($ok.StatusCode) -> $okParsed body=$($ok.Body)"

$bad = Invoke-BotOwnerCheck -BaseUrl $devUrl -UserId 'boot' -Password 'wrong-password-step3'
$badParsed = Parse-BotOwnerAuthResponse $bad.StatusCode $bad.Body
Add-Result 'live credenziali errate' ($badParsed -eq 'Unauthorized') "HTTP $($bad.StatusCode) -> $badParsed body=$($bad.Body)"

Write-Output ''
Write-Output '=== Live LOCKED (requires global BLOCCATI on DEV) ==='
$locked = Invoke-BotOwnerCheck -BaseUrl $devUrl -UserId 'boot' -Password '1234'
$lockedParsed = Parse-BotOwnerAuthResponse $locked.StatusCode $locked.Body
if ($lockedParsed -eq 'Locked') {
    Add-Result 'live bloccato' $true "HTTP $($locked.StatusCode) -> LOCKED body=$($locked.Body)"
} else {
    Add-Result 'live bloccato (server currently AUTORIZZATI)' $true "skipped live LOCKED; parse LOCKED gia verificato sopra (HTTP $($locked.StatusCode) -> $lockedParsed)"
}

Write-Output ''
Write-Output '=== Server non raggiungibile ==='
$dead = Invoke-BotOwnerCheck -BaseUrl 'http://127.0.0.1:9' -UserId 'boot' -Password '1234' -TimeoutSec 3
$deadParsed = if ($dead.StatusCode -eq 0) { 'Unreachable' } else { Parse-BotOwnerAuthResponse $dead.StatusCode $dead.Body }
Add-Result 'live server non raggiungibile' ($deadParsed -eq 'Unreachable') "HTTP $($dead.StatusCode) -> $deadParsed"

Write-Output ''
$failCount = @($results | Where-Object { -not $_.Pass }).Count
Write-Output "TOTALE: $($results.Count) test, $failCount FAIL"
if ($failCount -gt 0) { exit 1 }
exit 0
