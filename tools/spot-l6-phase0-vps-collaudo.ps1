# Fase 0 - SPOT L6 consumo post-gate (VPS Decisore / IIS Proactive).
# Eseguire DOPO Deploy Safe Decisore con fix 049d184+.
#
#   powershell -ExecutionPolicy Bypass -File .\tools\spot-l6-phase0-vps-collaudo.ps1
#
# Env: DECISORE_URL (default http://127.0.0.1), DECIDE_USERNAME, DECIDE_PASSWORD

$ErrorActionPreference = 'Stop'

function Ok($name, $detail) {
    if ($detail) { Write-Host "OK   $name - $detail" -ForegroundColor Green }
    else { Write-Host "OK   $name" -ForegroundColor Green }
}
function Fail($name, $detail) {
    if ($detail) { Write-Host "FAIL $name - $detail" -ForegroundColor Red }
    else { Write-Host "FAIL $name" -ForegroundColor Red }
    Write-Host "`nVERDICT: FAIL - Fase 0 collaudo VPS" -ForegroundColor Red
    exit 1
}

function Invoke-DecideSpot {
    param(
        [string]$Computer,
        [int]$Mazzo,
        [int]$ColpoMartingala,
        [string]$Pbt,
        [string]$ChosenColor,
        [string]$Stato = 'Sculping',
        [string]$Valore = '10'
    )
    $qs = @{
        USERNAME = $env:DECIDE_USERNAME
        PASSWORD = $env:DECIDE_PASSWORD
        COMPUTER = $Computer
        TAVOLO = '1'
        SALDO_INIZIALE = '1000'
        MARGINE = '0'
        COLPO_MARTINGALA = [string]$ColpoMartingala
        MAZZO = [string]$Mazzo
        PBT = $Pbt
        CHOSEN_COLOR = $ChosenColor
        VALORE_GIOCATO = $Valore
        STATO = $Stato
    }
    $query = ($qs.GetEnumerator() | ForEach-Object { "{0}={1}" -f $_.Key, [uri]::EscapeDataString([string]$_.Value) }) -join '&'
    $url = "$($env:DECISORE_URL.TrimEnd('/'))/api/proactive/decide?$query"
    $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 60
    $text = $res.Content.Trim()
    if ($text -notmatch '^\d+$') { throw "decide non numerico: $text" }
    return [int]$text
}

function Get-SpotBot {
    param([string]$Computer)
    $url = "$($env:DECISORE_URL.TrimEnd('/'))/api/proactive/security-filter/$([uri]::EscapeDataString($Computer))"
    return Invoke-RestMethod -Uri $url -UseBasicParsing -TimeoutSec 30
}

function Get-BotInt($bot, [string]$name) {
    $pascal = $bot.$name
    if ($null -ne $pascal) { return [int]$pascal }
    $camel = $bot.($name.Substring(0,1).ToLower() + $name.Substring(1))
    if ($null -ne $camel) { return [int]$camel }
    return 0
}

function Get-BotBool($bot, [string]$name) {
    $pascal = $bot.$name
    if ($null -ne $pascal) { return [bool]$pascal }
    $camel = $bot.($name.Substring(0,1).ToLower() + $name.Substring(1))
    if ($null -ne $camel) { return [bool]$camel }
    return $false
}

if (-not $env:DECISORE_URL) { $env:DECISORE_URL = 'http://127.0.0.1' }
if (-not $env:DECIDE_USERNAME) { $env:DECIDE_USERNAME = 'eugenio' }
if (-not $env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD = '123456' }

$pcGrant = 'SPOTF0_GRANT'
$pcHz = 'SPOTF0_HZ'
$pcIso1 = 'SPOTF0_ISO1'
$pcIso2 = 'SPOTF0_ISO2'
$pcCycle = 'SPOTF0_CYCLE'

Write-Host "`n=== Fase 0 - reset engine (collaudo isolato) ===" -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri "$($env:DECISORE_URL.TrimEnd('/'))/api/proactive/reset" -UseBasicParsing -TimeoutSec 30 | Out-Null
    Ok 'engine reset' 'state cleared for collaudo PCs'
}
catch {
    Fail 'engine reset' $_.Exception.Message
}

Write-Host "`n=== 1. Grant su 5->6 (StopL6=false) ===" -ForegroundColor Cyan
$h = 5000
$a = Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($a -eq 9) { Fail 'L5 loss 1' "decide=$a" }
$b = Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($b -eq 9) { Fail 'L5 loss 2' "decide=$b" }
$botPre = Get-SpotBot $pcGrant
if (-not (Get-BotBool $botPre 'SpotL6Authorized')) {
    Fail 'auth matura' "L5=$(Get-BotInt $botPre 'SpotL5LossCount') grant=$(Get-BotInt $botPre 'SpotL6GrantedCount')"
}
if ((Get-BotInt $botPre 'SpotL6GrantedCount') -ne 0) {
    Fail 'pre grant' "expected 0 got $(Get-BotInt $botPre 'SpotL6GrantedCount')"
}
Ok 'auth maturata' "L5=$(Get-BotInt $botPre 'SpotL5LossCount') authorized=$(Get-BotBool $botPre 'SpotL6Authorized')"

$c = Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 5 -Pbt 'B' -ChosenColor 'B'
if ($c -eq 9) { Fail '5->6 grant' "decide=$c" }
if ($c -eq 2) { Fail '5->6 grant' "StopL6 blocked (action=2)" }
$botPost = Get-SpotBot $pcGrant
if ((Get-BotInt $botPost 'SpotL6GrantedCount') -ne 1) {
    Fail 'grant++' "action=$c granted=$(Get-BotInt $botPost 'SpotL6GrantedCount')"
}
if ((Get-BotInt $botPost 'SpotL5LossCount') -ne 0) {
    Fail 'L5 azzerate' "got $(Get-BotInt $botPost 'SpotL5LossCount')"
}
Ok 'grant consumato' "granted=$(Get-BotInt $botPost 'SpotL6GrantedCount') L5=$(Get-BotInt $botPost 'SpotL5LossCount')"

Write-Host "`n=== 2. Hot Zone blocca 5->6 (grant invariato) ===" -ForegroundColor Cyan
$h = 6000
Invoke-DecideSpot -Computer $pcHz -Mazzo 1 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Invoke-DecideSpot -Computer $pcHz -Mazzo 2 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
$botHzPre = Get-SpotBot $pcHz
if (-not (Get-BotBool $botHzPre 'SpotL6Authorized')) { Fail 'HZ auth' 'not authorized after 2 L5' }
$dHz = Invoke-DecideSpot -Computer $pcHz -Mazzo 10 -ColpoMartingala 5 -Pbt 'B' -ChosenColor 'B'
if ($dHz -ne 2) { Fail 'HZ StopL6' "expected action=2 got $dHz" }
$botHzPost = Get-SpotBot $pcHz
if ((Get-BotInt $botHzPost 'SpotL6GrantedCount') -ne 0) { Fail 'HZ grant' "got $(Get-BotInt $botHzPost 'SpotL6GrantedCount')" }
if ((Get-BotInt $botHzPost 'SpotL5LossCount') -ne 2) { Fail 'HZ L5' "got $(Get-BotInt $botHzPost 'SpotL5LossCount')" }
if (-not (Get-BotBool $botHzPost 'SpotL6Authorized')) { Fail 'HZ auth preserved' 'authorized=false' }
Ok 'HZ blocked' "grant=0 L5=$(Get-BotInt $botHzPost 'SpotL5LossCount') auth=$(Get-BotBool $botHzPost 'SpotL6Authorized')"

Write-Host "`n=== 3. L1 senza L6 - auth/L5 restano ===" -ForegroundColor Cyan
$h = 7000
Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
$e = Invoke-DecideSpot -Computer $pcGrant -Mazzo ($h++) -ColpoMartingala 0 -Pbt 'B' -ChosenColor 'B'
if ($e -eq 9) { Fail 'L1 return' "decide=$e" }
$botL1 = Get-SpotBot $pcGrant
if ((Get-BotInt $botL1 'SpotL5LossCount') -lt 2) { Fail 'L1 L5 preserved' "L5=$(Get-BotInt $botL1 'SpotL5LossCount')" }
if (-not (Get-BotBool $botL1 'SpotL6Authorized')) { Fail 'L1 auth preserved' 'authorized=false' }
if ((Get-BotInt $botL1 'SpotL6GrantedCount') -ne 1) { Fail 'L1 grant preserved' "grant=$(Get-BotInt $botL1 'SpotL6GrantedCount')" }
Ok 'L1 senza L6' "L5=$(Get-BotInt $botL1 'SpotL5LossCount') auth=$(Get-BotBool $botL1 'SpotL6Authorized') grant=$(Get-BotInt $botL1 'SpotL6GrantedCount')"

Write-Host "`n=== 4. Isolamento bot ===" -ForegroundColor Cyan
Invoke-DecideSpot -Computer $pcIso1 -Mazzo 8001 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Invoke-DecideSpot -Computer $pcIso2 -Mazzo 9001 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
$b1 = Get-SpotBot $pcIso1
$b2 = Get-SpotBot $pcIso2
$l1 = Get-BotInt $b1 'SpotL5LossCount'
$l2 = Get-BotInt $b2 'SpotL5LossCount'
if ($l1 -ne 1 -or $l2 -ne 1) { Fail 'isolamento L5' "ISO1=$l1 ISO2=$l2" }
Ok 'isolamento bot' "ISO1 L5=$l1 ISO2 L5=$l2"

Write-Host "`n=== 5. Nessun decide=9 su baseline ===" -ForegroundColor Cyan
$baseline = Invoke-DecideSpot -Computer 'SPOTF0_BASE' -Mazzo 50 -ColpoMartingala 0 -Pbt 'B' -ChosenColor 'B' -Stato 'ATTESA'
if ($baseline -eq 9) { Fail 'decide=9' 'engine error' }
Ok 'no decide=9' "baseline action=$baseline"

Write-Host "`nVERDICT: PASS - Fase 0 collaudo VPS Decisore OK" -ForegroundColor Green
exit 0
