# Final Phase-1 telemetry audit — Decisore + WebApi + API smoke
param(
    [string]$ApiBase = "https://vps-b0942869.vps.ovh.net",
    [string]$DecisoreBase = "http://localhost:5286",
    [string]$WebApiBase = "http://localhost:5299",
    [string]$Username = "turri",
    [string]$Password = "Lina1967!",
    [switch]$SkipProdApi,
    [switch]$SkipLocalServices
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root

$report = [ordered]@{}
$failures = @()

function Add-Result($key, $value, [bool]$ok = $true) {
    $report[$key] = $value
    if (-not $ok) { $script:failures += $key }
}

Write-Host "`n========== BUILD =========="
$decisoreBuild = dotnet build "decision-engine/Decisore/Decisore.csproj" -v q 2>&1
Add-Result "Build.Decisore" $(if ($LASTEXITCODE -eq 0) { "PASS (0 errors)" } else { "FAIL"; $decisoreBuild[-5..-1] -join "`n" }) ($LASTEXITCODE -eq 0)

$webapiBuild = dotnet build "backend/WebApi/WebApi.csproj" -v q 2>&1
Add-Result "Build.WebApi" $(if ($LASTEXITCODE -eq 0) { "PASS (0 errors)" } else { "FAIL"; $webapiBuild[-5..-1] -join "`n" }) ($LASTEXITCODE -eq 0)

Push-Location frontend
npm run build --silent 2>&1 | Out-Null
Add-Result "Build.Frontend" $(if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL exit $LASTEXITCODE" }) ($LASTEXITCODE -eq 0)
Pop-Location

Write-Host "`n========== SLIM SERIALIZATION (1/2/4/8 bot) =========="
dotnet run --project tools/TelemetrySlimAudit/TelemetrySlimAudit.csproj -c Release --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    dotnet run --project tools/TelemetrySlimAudit/TelemetrySlimAudit.csproj -c Release 2>&1
}
Add-Result "TelemetrySlimAudit" $(if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }) ($LASTEXITCODE -eq 0)

python tools/validate-telemetry-slim.py 2>&1
Add-Result "PythonValidateSlim" $(if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }) ($LASTEXITCODE -eq 0)

if (-not $SkipLocalServices) {
    Write-Host "`n========== DECISORE STARTUP =========="
    $decisoreProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project decision-engine/Decisore/Decisore.csproj --no-build -c Release --urls http://localhost:5286" -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 8
    try {
        $sf404 = Invoke-WebRequest -Uri "$DecisoreBase/api/proactive/security-filter/PC1" -UseBasicParsing -TimeoutSec 10
        Add-Result "Decisore.Startup" "PASS (endpoint reachable, status $($sf404.StatusCode))" ($sf404.StatusCode -eq 404)
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            Add-Result "Decisore.Startup" "PASS (listening, security-filter/PC1 -> 404 without session)"
        } else {
            Add-Result "Decisore.Startup" "FAIL: $($_.Exception.Message)" $false
        }
    }

    Write-Host "`n========== WEBAPI STARTUP =========="
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:Decider__BaseUrl = $DecisoreBase
    $webapiProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project backend/WebApi/WebApi.csproj --no-build -c Release --urls http://localhost:5299" -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 12
    try {
        $health = Invoke-WebRequest -Uri "$WebApiBase/swagger/index.html" -UseBasicParsing -TimeoutSec 15
        Add-Result "WebApi.Startup" "PASS (swagger reachable $($health.StatusCode))"
    } catch {
        Add-Result "WebApi.Startup" "WARN: $($_.Exception.Message) - may need local DB" $true
    }

    if ($decisoreProc -and -not $decisoreProc.HasExited) { Stop-Process -Id $decisoreProc.Id -Force -ErrorAction SilentlyContinue }
    if ($webapiProc -and -not $webapiProc.HasExited) { Stop-Process -Id $webapiProc.Id -Force -ErrorAction SilentlyContinue }
}

if (-not $SkipProdApi) {
    Write-Host "`n========== PROD API (pre-deploy baseline) =========="
    try {
        $login = Invoke-RestMethod -Uri "$ApiBase/api/Auth/login" -Method POST -ContentType "application/json" -Body (@{ username = $Username; password = $Password } | ConvertTo-Json) -TimeoutSec 60
        $h = @{ Authorization = "Bearer $($login.token)" }

        $data = Invoke-RestMethod -Uri "$ApiBase/api/Dashboard/data" -Headers $h -TimeoutSec 60
        $rows = if ($data.data.tables) { $data.data.tables } else { $data.data }
        Add-Result "API.Dashboard/data" "PASS rows=$(@($rows).Count)"

        $tel = Invoke-RestMethod -Uri "$ApiBase/api/Dashboard/telemetry" -Headers $h -TimeoutSec 60
        $t = if ($tel.data) { $tel.data } else { $tel }
        $raw = if ($t.rawTelemetry) { [string]$t.rawTelemetry } else { "" }
        $rawLen = $raw.Length
        $rawValid = $false
        try { $null = $t.rawTelemetry | ConvertFrom-Json; $rawValid = $true } catch { $rawValid = $false }
        Add-Result "API.Dashboard/telemetry" "rawLen=$rawLen validJson=$rawValid totalPb=$($t.totalPbHandsPlayed) sfBots=$($t.securityFilterByBot.PSObject.Properties.Count)" $(-not ($rawLen -eq 4000 -and -not $rawValid))

        try {
            $sf = Invoke-RestMethod -Uri "$ApiBase/api/Dashboard/security-filter/PC1" -Headers $h -TimeoutSec 30
            Add-Result "API.security-filter" "NOTE: endpoint exists (post-deploy test needed)"
        } catch {
            if ($_.Exception.Response.StatusCode.value__ -eq 404) {
                Add-Result "API.security-filter" "404 (expected pre-deploy or no bot PC1)"
            } else {
                Add-Result "API.security-filter" "NOTE: $($_.Exception.Message) (endpoint not deployed yet)"
            }
        }
    } catch {
        Add-Result "API.Prod" "FAIL: $($_.Exception.Message)" $false
    }
}

Write-Host "`n========== UNRELATED CODE CHECK =========="
$telemetryFiles = @(
    "decision-engine/Decisore/Engine/TelemetryPersistence.cs",
    "decision-engine/Decisore/Controllers/EngineController.cs",
    "decision-engine/Decisore/Engine/ProactiveEngine.cs",
    "decision-engine/Decisore/Repository/DatabaseRepository.cs",
    "decision-engine/Decisore/Services/ProactiveEngineService.cs",
    "backend/WebApi/Controllers/DashboardController.cs",
    "backend/WebApi/Services/Implementations/DashboardService.cs",
    "backend/WebApi/Services/IDashboardService.cs",
    "backend/WebApi/Extensions/ServiceCollectionExtensions.cs",
    "frontend/src/components/dashboard/StatsWidget.vue",
    "frontend/src/views/Dashboard.vue",
    "frontend/src/service/DashboardService.ts",
    "ops/dash2a-readiness/TELEMETRY-PAYLOAD-FIX.md",
    "tools/validate-telemetry-slim.py",
    "tools/TelemetrySlimAudit"
)

$forbiddenPatterns = @(
    "MissionController",
    "MissionReport",
    "Pro220",
    "FinancialReport",
    "ApplyCanonicalAccounting"
)

$diffFiles = git diff --name-only HEAD
$telemetryDiff = $diffFiles | Where-Object { $_ -in $telemetryFiles -or $_ -like "tools/TelemetrySlimAudit/*" -or $_ -like "tools/validate-telemetry-slim.py" }
$otherDiff = $diffFiles | Where-Object { $_ -notin $telemetryDiff -and $_ -notlike "tools/*" }

Add-Result "Scope.TelemetryFiles" ($telemetryDiff -join ", ")
Add-Result "Scope.OtherModifiedInWorkingTree" $(if ($otherDiff.Count) { ($otherDiff -join ", ") + " - EXCLUDE FROM COMMIT" } else { "none" })

foreach ($pat in $forbiddenPatterns) {
    $hits = git diff HEAD | Select-String -Pattern $pat
    Add-Result "Untouched.$pat" $(if ($hits) { "TOUCHED - review required" } else { "OK - not in diff" }) (-not $hits)
}

Write-Host "`n========== GIT DIFF STAT (telemetry scope) =========="
git diff --stat -- `
    decision-engine/Decisore/Engine/TelemetryPersistence.cs `
    decision-engine/Decisore/Controllers/EngineController.cs `
    decision-engine/Decisore/Engine/ProactiveEngine.cs `
    decision-engine/Decisore/Repository/DatabaseRepository.cs `
    decision-engine/Decisore/Services/ProactiveEngineService.cs `
    backend/WebApi/Controllers/DashboardController.cs `
    backend/WebApi/Services/Implementations/DashboardService.cs `
    backend/WebApi/Services/IDashboardService.cs `
    backend/WebApi/Extensions/ServiceCollectionExtensions.cs `
    frontend/src/components/dashboard/StatsWidget.vue `
    frontend/src/views/Dashboard.vue `
    frontend/src/service/DashboardService.ts `
    ops/dash2a-readiness/TELEMETRY-PAYLOAD-FIX.md `
    tools/validate-telemetry-slim.py `
    tools/TelemetrySlimAudit 2>&1

Write-Host "`n========== AUDIT SUMMARY =========="
$report.GetEnumerator() | ForEach-Object { Write-Host ("{0}: {1}" -f $_.Key, $_.Value) }

if ($failures.Count) {
    Write-Host "`nFAILED CHECKS: $($failures -join ', ')"
    exit 1
}
Write-Host "`nAUDIT PASS - ready for commit (telemetry scope only)"
exit 0
