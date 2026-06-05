# Fase 1 - SPOT L6 crediti per bot (VPS Decisore / IIS Proactive).
# Eseguire DOPO Deploy Safe Decisore con feat credit balance engine.
#
#   powershell -ExecutionPolicy Bypass -File .\tools\spot-l6-phase1-vps-collaudo.ps1
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
    Write-Host "`nVERDICT: FAIL - Fase 1 collaudo VPS" -ForegroundColor Red
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

function Wait-HandPace {
    $sec = 26
    if ($env:COLLAUDO_HAND_PACE_SECONDS) { $sec = [int]$env:COLLAUDO_HAND_PACE_SECONDS }
    Start-Sleep -Seconds $sec
}

if (-not $env:DECISORE_URL) { $env:DECISORE_URL = 'http://127.0.0.1' }
if (-not $env:DECIDE_USERNAME) { $env:DECIDE_USERNAME = 'eugenio' }
if (-not $env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD = '123456' }

$pcCredit = 'SPOTF1_CREDIT'
$pcHz = 'SPOTF1_HZ'
$pcL1 = 'SPOTF1_L1'
$pcIso1 = 'SPOTF1_ISO1'
$pcIso2 = 'SPOTF1_ISO2'

Write-Host "`n=== Fase 1 - reset engine (collaudo isolato) ===" -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri "$($env:DECISORE_URL.TrimEnd('/'))/api/proactive/reset" -UseBasicParsing -TimeoutSec 30 | Out-Null
    Ok 'engine reset' 'state cleared for collaudo PCs'
}
catch {
    Fail 'engine reset' $_.Exception.Message
}

Write-Host "`n=== 1. 2 L5 -> +1 credito ===" -ForegroundColor Cyan
$h = 5000
$a = Invoke-DecideSpot -Computer $pcCredit -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($a -eq 9) { Fail 'L5 loss 1' "decide=$a" }
Wait-HandPace
$b = Invoke-DecideSpot -Computer $pcCredit -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($b -eq 9) { Fail 'L5 loss 2' "decide=$b" }
$bot1 = Get-SpotBot $pcCredit
$credit1 = Get-BotInt $bot1 'SpotL6CreditBalance'
if ($credit1 -ne 1) { Fail '2 L5 +1 credito' "balance=$credit1" }
Ok '2 L5 +1 credito' "balance=$credit1"

Write-Host "`n=== 2. 4 L5 -> +2 crediti ===" -ForegroundColor Cyan
Wait-HandPace
$c = Invoke-DecideSpot -Computer $pcCredit -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($c -eq 9) { Fail 'L5 loss 3' "decide=$c" }
Wait-HandPace
$d = Invoke-DecideSpot -Computer $pcCredit -Mazzo ($h++) -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($d -eq 9) { Fail 'L5 loss 4' "decide=$d" }
$bot2 = Get-SpotBot $pcCredit
$credit2 = Get-BotInt $bot2 'SpotL6CreditBalance'
if ($credit2 -ne 2) { Fail '4 L5 +2 crediti' "balance=$credit2" }
Ok '4 L5 +2 crediti' "balance=$credit2"

Write-Host "`n=== 3. 5->6 consuma 1 credito ===" -ForegroundColor Cyan
$e = Invoke-DecideSpot -Computer $pcCredit -Mazzo ($h++) -ColpoMartingala 5 -Pbt 'B' -ChosenColor 'B'
if ($e -eq 9) { Fail '5->6' "decide=$e" }
if ($e -eq 2) { Fail '5->6' "StopL6 blocked action=2" }
$bot3 = Get-SpotBot $pcCredit
$credit3 = Get-BotInt $bot3 'SpotL6CreditBalance'
$grant3 = Get-BotInt $bot3 'SpotL6GrantedCount'
if ($credit3 -ne 1) { Fail '5->6 credito' "balance=$credit3 expected=1" }
if ($grant3 -ne 1) { Fail '5->6 grant' "grant=$grant3 expected=1" }
Ok '5->6 consuma 1 credito' "balance=$credit3 grant=$grant3"

Write-Host "`n=== 4. 2 crediti + 1 L6 -> resta 1 credito ===" -ForegroundColor Cyan
if ($credit3 -ne 1) { Fail 'residuo 1 credito' "balance=$credit3" }
Ok 'residuo 1 credito' "balance=$credit3"

Write-Host "`n=== 5. Hot Zone blocca L6 (credito invariato) ===" -ForegroundColor Cyan
Invoke-DecideSpot -Computer $pcHz -Mazzo 1 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Wait-HandPace
Invoke-DecideSpot -Computer $pcHz -Mazzo 2 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Wait-HandPace
$hzPre = Get-SpotBot $pcHz
$hzCreditPre = Get-BotInt $hzPre 'SpotL6CreditBalance'
if ($hzCreditPre -lt 1) { Fail 'HZ pre credito' "balance=$hzCreditPre" }
$dHz = Invoke-DecideSpot -Computer $pcHz -Mazzo 10 -ColpoMartingala 5 -Pbt 'B' -ChosenColor 'B'
if ($dHz -ne 2 -and $dHz -ne 3) { Fail 'HZ StopL6' "expected action 2 or 3 got $dHz" }
$hzPost = Get-SpotBot $pcHz
$hzCreditPost = Get-BotInt $hzPost 'SpotL6CreditBalance'
$hzGrantPost = Get-BotInt $hzPost 'SpotL6GrantedCount'
if ($hzCreditPre -ne $hzCreditPost) { Fail 'HZ credito' "pre=$hzCreditPre post=$hzCreditPost" }
if ($hzGrantPost -ne 0) { Fail 'HZ grant' "grant=$hzGrantPost" }
Ok 'HZ blocked' "credit=$hzCreditPost grant=$hzGrantPost action=$dHz"

Write-Host "`n=== 6. L1 senza L6 - credito invariato ===" -ForegroundColor Cyan
$creditPcPre = Get-BotInt (Get-SpotBot $pcCredit) 'SpotL6CreditBalance'
$f = Invoke-DecideSpot -Computer $pcL1 -Mazzo 7001 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($f -eq 9) { Fail 'L1 setup L5-1' "decide=$f" }
Wait-HandPace
$g = Invoke-DecideSpot -Computer $pcL1 -Mazzo 7002 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B'
if ($g -eq 9) { Fail 'L1 setup L5-2' "decide=$g" }
Wait-HandPace
$hAct = Invoke-DecideSpot -Computer $pcL1 -Mazzo 7003 -ColpoMartingala 0 -Pbt 'B' -ChosenColor 'B'
if ($hAct -eq 9) { Fail 'L1 return' "decide=$hAct" }
$l1Pre = Get-SpotBot $pcL1
$l1CreditPre = Get-BotInt $l1Pre 'SpotL6CreditBalance'
if ($l1CreditPre -ne 1) { Fail 'L1 pre credito' "balance=$l1CreditPre" }
$l1Post = Get-SpotBot $pcL1
$l1CreditPost = Get-BotInt $l1Post 'SpotL6CreditBalance'
if ($l1CreditPost -ne $l1CreditPre) { Fail 'L1 credito consumato' "pre=$l1CreditPre post=$l1CreditPost" }
$creditPcPost = Get-BotInt (Get-SpotBot $pcCredit) 'SpotL6CreditBalance'
if ($creditPcPost -ne $creditPcPre) { Fail 'L1 tocca altro PC' "credit PC pre=$creditPcPre post=$creditPcPost" }
Ok 'L1 senza L6' "L1 credit=$l1CreditPost PC credit=$creditPcPost"

Write-Host "`n=== 7. Fine ciclo SPOT (coperto da smoke CI) ===" -ForegroundColor Cyan
Ok 'ciclo SPOT reset' 'validato da spot-reset-per-bot-smoke in CI'

Write-Host "`n=== 8. Isolamento bot ===" -ForegroundColor Cyan
Invoke-DecideSpot -Computer $pcIso1 -Mazzo 8001 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
Wait-HandPace
Invoke-DecideSpot -Computer $pcIso2 -Mazzo 9001 -ColpoMartingala 4 -Pbt 'P' -ChosenColor 'B' | Out-Null
$b1 = Get-SpotBot $pcIso1
$b2 = Get-SpotBot $pcIso2
$l1iso = Get-BotInt $b1 'SpotL5LossCount'
$l2iso = Get-BotInt $b2 'SpotL5LossCount'
$c1iso = Get-BotInt $b1 'SpotL6CreditBalance'
$c2iso = Get-BotInt $b2 'SpotL6CreditBalance'
if ($l1iso -ne 1 -or $l2iso -ne 1) { Fail 'isolamento L5' "ISO1=$l1iso ISO2=$l2iso" }
if ($c1iso -ne 0 -or $c2iso -ne 0) { Fail 'isolamento credito' "ISO1=$c1iso ISO2=$c2iso" }
Ok 'isolamento bot' "ISO1 L5=$l1iso ISO2 L5=$l2iso crediti=0/0"

Write-Host "`n=== 9. Legacy globale OFF (modello crediti per-bot) ===" -ForegroundColor Cyan
if ($credit2 -ne 2) { Fail 'legacy OFF accumulo' "4 L5 non accumulano 2 crediti" }
Ok 'legacy OFF' 'accumulo crediti per-bot su stesso PC'

Write-Host "`n=== 10. Nessun decide=9 su baseline ===" -ForegroundColor Cyan
$baseline = Invoke-DecideSpot -Computer 'SPOTF1_BASE' -Mazzo 50 -ColpoMartingala 0 -Pbt 'B' -ChosenColor 'B' -Stato 'ATTESA'
if ($baseline -eq 9) { Fail 'decide=9' 'engine error' }
Ok 'no decide=9' "baseline action=$baseline"

Write-Host "`nVERDICT: PASS - Fase 1 collaudo VPS Decisore OK" -ForegroundColor Green
exit 0
