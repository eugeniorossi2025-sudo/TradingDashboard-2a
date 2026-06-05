# Collaudo Control Room override - eseguire SOLO dalla VPS Decisore (IP autorizzato su SQL 1434).
# Prerequisito: patch Decisore deployata; DB runtime 51.83.159.175:1434.
#
# Uso:
#   .\tools\control-room-vps-collaudo.ps1
#
# Env opzionali:
#   COLLAUDO_CONNECTION_STRING  - default: C:\Decisore\appsettings.json
#   DECISORE_URL                - default: http://127.0.0.1
#   DECIDE_USERNAME / DECIDE_PASSWORD

$ErrorActionPreference = 'Stop'

function Write-Step($n, $msg) { Write-Host "`n=== $n. $msg ===" -ForegroundColor Cyan }

$repoRoot = Split-Path -Parent $PSScriptRoot
$decisoreAppsettings = @(
    'C:\Decisore\appsettings.json',
    (Join-Path $repoRoot 'decision-engine\Decisore\appsettings.json')
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $env:COLLAUDO_CONNECTION_STRING -and $decisoreAppsettings) {
    $json = Get-Content $decisoreAppsettings -Raw | ConvertFrom-Json
    $env:COLLAUDO_CONNECTION_STRING = $json.ConnectionStrings.DefaultConnection
}

if (-not $env:DECISORE_URL) {
    $env:DECISORE_URL = 'http://127.0.0.1'
}

if (-not $env:DECIDE_USERNAME) { $env:DECIDE_USERNAME = 'eugenio' }
if (-not $env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD = '123456' }

if ([string]::IsNullOrWhiteSpace($env:COLLAUDO_CONNECTION_STRING)) {
    Write-Error 'COLLAUDO_CONNECTION_STRING mancante. Impostare env o appsettings Decisore su VPS.'
}

Write-Step 1 'Verifica DB + CONTINUA (AC0 one-shot)'
Write-Step 2 'Verifica AZZERA (AC2 one-shot) incluso nello smoke E2E'
Write-Step 3 'Verifica isolamento PC incluso nello smoke E2E'

$e2eProject = Join-Path $repoRoot 'tools\control-room-e2e-collaudo\ControlRoomE2eCollaudo.csproj'
if (-not (Test-Path $e2eProject)) {
    Write-Error "Progetto collaudo non trovato: $e2eProject"
}

dotnet run --project $e2eProject
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nVERDICT: FAIL - STOP DEPLOY (collaudo E2E VPS non superato)" -ForegroundColor Red
    exit 1
}

Write-Step 4 'Verifica non regressione (statica, da repo se presente)'
if (Test-Path (Join-Path $repoRoot '.git')) {
    Push-Location $repoRoot
    try {
        $dirtyEngine = git diff --name-only HEAD -- 'decision-engine/Decisore/Engine/ProactiveEngine.cs'
        $dirtyMission = git diff --name-only HEAD -- '**/Mission*' '**/Accounting*' '**/MissionReport*'
        if ($dirtyEngine) { Write-Warning "ProactiveEngine.cs modificato nel working tree: $dirtyEngine" }
        else { Write-Host 'OK   ProactiveEngine.cs non modificato (HEAD diff)' -ForegroundColor Green }
        if ($dirtyMission) { Write-Warning "File missioni/contabilita modificati: $($dirtyMission -join ', ')" }
        else { Write-Host 'OK   missioni/report/contabilita non modificati (HEAD diff)' -ForegroundColor Green }
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host 'SKIP git audit (repo non presente su VPS - verificare manualmente checklist 4)' -ForegroundColor Yellow
}

Write-Host "`nVERDICT: PASS - collaudo VPS completato. WebApi/UI ancora da collaudare." -ForegroundColor Green
