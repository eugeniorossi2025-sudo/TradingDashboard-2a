# Collaudo live: SPOT L6 OFF = modulo assente (PC2).
# Usa WebApi per toggle OFF + Decisore per decide/telemetry bot.
param(
    [string]$ApiUrl = $(if ($env:DASH2A_API_URL) { $env:DASH2A_API_URL } else { 'https://vps-b0942869.vps.ovh.net' }),
    [string]$DecisoreUrl = $(if ($env:DECISORE_URL) { $env:DECISORE_URL } else { 'http://51.178.16.37' }),
    [string]$Computer = $(if ($env:COLLAUDO_PC) { $env:COLLAUDO_PC } else { 'PC2' }),
    [string]$Username = $(if ($env:DASH2A_USER) { $env:DASH2A_USER } else { 'admin' }),
    [string]$Password = $(if ($env:DASH2A_PASSWORD) { $env:DASH2A_PASSWORD } else { 'Admin@123456' }),
    [string]$DecideUsername = $(if ($env:DECIDE_USERNAME) { $env:DECIDE_USERNAME } else { 'eugenio' }),
    [string]$DecidePassword = $(if ($env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD } else { '123456' })
)

$ErrorActionPreference = 'Stop'
$ApiUrl = $ApiUrl.TrimEnd('/')
$DecisoreUrl = $DecisoreUrl.TrimEnd('/')

function Ok($name, $detail = '') {
    Write-Host "OK   $name$(if ($detail) { " - $detail" })"
}
function Fail($name, $detail = '') {
    Write-Host "FAIL $name$(if ($detail) { " - $detail" })"
    Write-Host "`nVERDICT FAIL - spot-l6-off-telemetry-pc2-collaudo"
    exit 1
}

function Get-BotField($bot, [string]$pascal) {
    $camel = $pascal.Substring(0,1).ToLower() + $pascal.Substring(1)
    if ($null -ne $bot.$pascal) { return $bot.$pascal }
    if ($null -ne $bot.$camel) { return $bot.$camel }
    return $null
}

function Invoke-Decide {
    param([string]$Computer, [int]$Mazzo, [int]$Martingala, [string]$Pbt, [string]$ChosenColor)
    $qs = @{
        USERNAME = $DecideUsername
        PASSWORD = $DecidePassword
        COMPUTER = $Computer
        TAVOLO = '1'
        SALDO_INIZIALE = '1000'
        MARGINE = '0'
        COLPO_MARTINGALA = [string]$Martingala
        MAZZO = [string]$Mazzo
        PBT = $Pbt
        CHOSEN_COLOR = $ChosenColor
        VALORE_GIOCATO = '10'
        STATO = 'Sculping'
    }
    $query = ($qs.GetEnumerator() | ForEach-Object { "{0}={1}" -f $_.Key, [uri]::EscapeDataString([string]$_.Value) }) -join '&'
    $url = "$DecisoreUrl/api/proactive/decide?$query"
    $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 60
    $text = $res.Content.Trim()
    if ($text -notmatch '^\d+$') { throw "decide non numerico: $text" }
    return [int]$text
}

function Wait-HandPace {
    $sec = 26
    if ($env:COLLAUDO_HAND_PACE_SECONDS) { $sec = [int]$env:COLLAUDO_HAND_PACE_SECONDS }
    Start-Sleep -Seconds $sec
}

function Get-SpotBot([string]$Computer) {
    $url = "$DecisoreUrl/api/proactive/security-filter/$([uri]::EscapeDataString($Computer))"
    try {
        return Invoke-RestMethod -Uri $url -UseBasicParsing -TimeoutSec 30
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) { return $null }
        throw
    }
}

Write-Host "=== login WebApi ===" -ForegroundColor Cyan
$login = Invoke-RestMethod -Uri "$ApiUrl/api/Auth/login" -Method POST -ContentType 'application/json' -Body (@{ Username = $Username; Password = $Password } | ConvertTo-Json)
$token = $login.data.token
if (-not $token) { $token = $login.token }
if ([string]::IsNullOrWhiteSpace($token)) { Fail 'login' }
$headers = @{ Authorization = "Bearer $token" }
Ok 'login'

Write-Host "=== SPOT L6 per bot OFF ===" -ForegroundColor Cyan
$off = Invoke-RestMethod -Uri "$ApiUrl/api/spot-l6-per-bot" -Method PUT -ContentType 'application/json' -Headers $headers -Body (@{ enabled = $false } | ConvertTo-Json)
$enabled = $off.data.enabled
if ($enabled -ne $false) { Fail 'spot-l6-per-bot OFF' "enabled=$enabled" }
Ok 'spot-l6-per-bot OFF'

Write-Host "=== Decisore reset (reload config DB) ===" -ForegroundColor Cyan
Invoke-WebRequest -Uri "$DecisoreUrl/api/proactive/reset" -UseBasicParsing -TimeoutSec 30 | Out-Null
Ok 'decisore reset + config reload'

Write-Host "=== L5 losses (COLPO_MARTINGALA=4 => engine L5) ===" -ForegroundColor Cyan
$h = 9100
$a1 = Invoke-Decide -Computer $Computer -Mazzo ($h++) -Martingala 4 -Pbt 'P' -ChosenColor 'B'
if ($a1 -eq 9) { Fail 'decide L5 loss 1' "action=$a1" }
if ($a1 -eq 3) { Fail 'decide L5 loss 1 no pause' "action=$a1" }
Wait-HandPace
$a2 = Invoke-Decide -Computer $Computer -Mazzo ($h++) -Martingala 4 -Pbt 'P' -ChosenColor 'B'
if ($a2 -eq 9) { Fail 'decide L5 loss 2' "action=$a2" }
if ($a2 -eq 3) { Fail 'decide L5 loss 2 no pause' "action=$a2" }
Ok 'L5 losses senza pausa SPOT' "actions=$a1,$a2"

$bot = Get-SpotBot $Computer
$l5Played = [int](Get-BotField $bot 'SpotL5PlayedCount')
$l5Loss = [int](Get-BotField $bot 'SpotL5LossCount')
$credit = [int](Get-BotField $bot 'SpotL6CreditBalance')
$auth = [bool](Get-BotField $bot 'SpotL6Authorized')
$nextL6 = [bool](Get-BotField $bot 'NextL5LossWillAuthorizeL6')
$pb = [int](Get-BotField $bot 'SpotPbHandsPlayed')

if ($l5Played -ne 0) { Fail 'SpotL5PlayedCount=0' "count=$l5Played" }
Ok 'SpotL5PlayedCount=0'

if ($l5Loss -ne 0) { Fail 'SpotL5LossCount=0' "count=$l5Loss" }
Ok 'SpotL5LossCount=0'

if ($credit -ne 0) { Fail 'SpotL6CreditBalance=0' "balance=$credit" }
Ok 'SpotL6CreditBalance=0'

if ($auth -ne $false) { Fail 'SpotL6Authorized=false' "auth=$auth" }
Ok 'SpotL6Authorized=false'

if ($nextL6 -ne $false) { Fail 'NextL5LossWillAuthorizeL6=false' "next=$nextL6" }
Ok 'NextL5LossWillAuthorizeL6=false'

if ($pb -ne 0) { Fail 'SpotPbHandsPlayed=0' "pb=$pb" }
Ok 'SpotPbHandsPlayed=0 (modulo SPOT fermo)'

Write-Host "=== L5->L6 libero (COLPO_MARTINGALA=5 => engine L6) ===" -ForegroundColor Cyan
Wait-HandPace
$l6 = Invoke-Decide -Computer $Computer -Mazzo ($h++) -Martingala 5 -Pbt 'P' -ChosenColor 'B'
if ($l6 -ne 0) { Fail 'L5->L6 ActionCode=0' "action=$l6 (expected 0)" }
Ok 'L5->L6 ActionCode=0' "action=$l6"

$after = Get-SpotBot $Computer
$creditAfter = [int](Get-BotField $after 'SpotL6CreditBalance')
$grantAfter = [int](Get-BotField $after 'SpotL6GrantedCount')
if ($creditAfter -ne 0) { Fail 'crediti restano 0 post L6' "balance=$creditAfter" }
if ($grantAfter -ne 0) { Fail 'nessun grant L6' "grant=$grantAfter" }
Ok 'nessun credito/grant SPOT post L6' "balance=$creditAfter grant=$grantAfter"

Write-Host "`nVERDICT PASS - spot-l6-off-telemetry-pc2-collaudo ($Computer)" -ForegroundColor Green
