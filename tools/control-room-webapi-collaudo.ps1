# Collaudo WebApi Control Room — eseguire DOPO PASS collaudo Decisore VPS.
# Prerequisito: WebApi patch deployata su VPS backend (51.83.159.175).

param(
    [string]$ApiUrl = $(if ($env:DASH2A_API_URL) { $env:DASH2A_API_URL } else { 'https://vps-b0942869.vps.ovh.net' }),
    [string]$Username = $(if ($env:DASH2A_USER) { $env:DASH2A_USER } else { 'admin' }),
    [string]$Password = $(if ($env:DASH2A_PASSWORD) { $env:DASH2A_PASSWORD } else { 'Admin@123456' }),
    [string]$TestPc = 'CR_E2E_PC2',
    [string]$DecisoreUrl = $(if ($env:DECISORE_URL) { $env:DECISORE_URL } else { 'http://51.178.16.37' }),
    [string]$DecideUsername = $(if ($env:DECIDE_USERNAME) { $env:DECIDE_USERNAME } else { 'eugenio' }),
    [string]$DecidePassword = $(if ($env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD } else { '123456' })
)

$ErrorActionPreference = 'Stop'
$ApiUrl = $ApiUrl.TrimEnd('/')

function Invoke-Decide([string]$computer) {
    $qs = @{
        USERNAME = $DecideUsername
        PASSWORD = $DecidePassword
        COMPUTER = $computer
        TAVOLO = '1'
        SALDO_INIZIALE = '1000'
        MARGINE = '0'
        COLPO_MARTINGALA = '0'
        MAZZO = '5'
        PBT = 'P'
        VALORE_GIOCATO = '10'
        STATO = 'ATTESA'
    }
    $query = ($qs.GetEnumerator() | ForEach-Object { "{0}={1}" -f $_.Key, [uri]::EscapeDataString([string]$_.Value) }) -join '&'
    $res = Invoke-WebRequest -Uri "$DecisoreUrl/api/proactive/decide?$query" -UseBasicParsing -TimeoutSec 60
    return [int]$res.Content.Trim()
}

Write-Host "=== WebApi login ===" -ForegroundColor Cyan
$loginBody = @{ Username = $Username; Password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Uri "$ApiUrl/api/Auth/login" -Method POST -ContentType 'application/json' -Body $loginBody
$token = $login.data.token ?? $login.token ?? $login.Data.Token
if ([string]::IsNullOrWhiteSpace($token)) { throw 'Login failed: no JWT token' }
$headers = @{ Authorization = "Bearer $token" }
Write-Host "OK   login"

Write-Host "=== POST continue ===" -ForegroundColor Cyan
$continue = Invoke-RestMethod -Uri "$ApiUrl/api/control-room/commands/continue" -Method POST -ContentType 'application/json' -Headers $headers -Body (@{ pc = $TestPc } | ConvertTo-Json)
if ($continue.success -eq $false) { throw "continue failed: $($continue.message)" }
Write-Host "OK   continue queued pc=$TestPc actionCode=$($continue.data.actionCode)"

$decide0 = Invoke-Decide $TestPc
if ($decide0 -ne 0) { throw "decide after continue expected 0 got $decide0" }
Write-Host "OK   decide after continue = 0"

$decide0b = Invoke-Decide $TestPc
if ($decide0b -eq 9) { throw 'decide=9 after continue consume' }
Write-Host "OK   second decide = $decide0b (engine normal)"

Write-Host "=== POST reset-martingale ===" -ForegroundColor Cyan
$reset = Invoke-RestMethod -Uri "$ApiUrl/api/control-room/commands/reset-martingale" -Method POST -ContentType 'application/json' -Headers $headers -Body (@{ pc = $TestPc } | ConvertTo-Json)
if ($reset.success -eq $false) { throw "reset-martingale failed: $($reset.message)" }
Write-Host "OK   reset-martingale queued pc=$TestPc actionCode=$($reset.data.actionCode)"

$decide2 = Invoke-Decide $TestPc
if ($decide2 -ne 2) { throw "decide after reset expected 2 got $decide2" }
Write-Host "OK   decide after reset = 2"

$decide2b = Invoke-Decide $TestPc
if ($decide2b -eq 9) { throw 'decide=9 after reset consume' }
Write-Host "OK   second decide = $decide2b (engine normal)"

Write-Host "`nVERDICT PASS — WebApi Control Room endpoints + decide coerenti" -ForegroundColor Green
