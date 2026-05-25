param(
    [switch]$Run,
    [switch]$SmokeOnly,
    [switch]$SkipBuild,
    [string]$ApiBaseUrl = "http://localhost:5299",
    [bool]$UseLocalDb = $true,
    [string]$LocalDbName = "Dash2A_LocalProdLike",
    [string]$AspNetCoreEnvironment = "LocalProdLike"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-Dash2Safety {
    Write-Step "Verifica repo DASH2 e sicurezza Firebase"

    $remote = git remote get-url origin
    if ($remote -notmatch "eugeniorossi2025-sudo/TradingDashboard-2a") {
        throw "Repo non sicuro: origin non e TradingDashboard-2a ($remote)"
    }

    $branch = git branch --show-current
    if ([string]::IsNullOrWhiteSpace($branch)) {
        throw "Repo non sicuro: HEAD detached"
    }

    $firebaseProject = (Get-Content "frontend/.firebaserc" -Raw | ConvertFrom-Json).projects.default
    if ($firebaseProject -ne "eugenio-dashboard-2") {
        throw "Firebase non sicuro: project default=$firebaseProject"
    }

    $dangerPattern = "dashboard-1|firebase\s+deploy|firebase\s+use|old endpoint|dirty fallback"
    $allowedExtensions = @(".json", ".yml", ".yaml", ".env", ".js", ".cjs", ".mjs", ".ts", ".vue", ".md")
    $excludedDirs = @("\node_modules\", "\dist\", "\.git\", "\coverage\", "\.cache\")
    $dangerous = Get-ChildItem "frontend" -Recurse -File |
        Where-Object {
            $path = $_.FullName
            ($allowedExtensions -contains $_.Extension) -and
            -not ($excludedDirs | Where-Object { $path -like "*$_*" })
        } |
        Select-String -Pattern $dangerPattern -CaseSensitive:$false
    if ($dangerous) {
        throw "Firebase non sicuro: trovati riferimenti pericolosi nel frontend"
    }

    Write-Host "Repo: OK ($remote)"
    Write-Host "Branch: OK ($branch)"
    Write-Host "Firebase: OK ($firebaseProject)"
}

function Stop-Port {
    param([int]$Port)

    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    foreach ($connection in $connections) {
        $pidToStop = $connection.OwningProcess
        if ($pidToStop -and $pidToStop -ne $PID) {
            $process = Get-Process -Id $pidToStop -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stop porta $Port PID $pidToStop ($($process.ProcessName))"
                Stop-Process -Id $pidToStop -Force
            }
        }
    }
}

function Assert-LocalFrontendApi {
    Write-Step "Verifica API frontend locale"

    $envFile = "frontend/.env"
    if (Test-Path $envFile) {
        $remoteApi = Select-String -Path $envFile -Pattern "VITE_API_BASE_URL\s*=\s*https?://(?!localhost|127\.0\.0\.1)" -CaseSensitive:$false
        if ($remoteApi) {
            Write-Host "Frontend .env punta remoto, ma Restart APP -Run forza VITE_API_BASE_URL=$ApiBaseUrl" -ForegroundColor Yellow
        }
    }

    Write-Host "Frontend API effettiva: $ApiBaseUrl"
}

function Test-RealLogin {
    param([string]$BaseUrl)

    Write-Step "Verifica login reale API"
    $body = @{
        username = "admin"
        password = "Admin@123456"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/Auth/login" -Method Post -ContentType "application/json" -Body $body -TimeoutSec 15
        if (-not $response.token) {
            throw "Login response senza token"
        }
        Write-Host "Login reale: OK token ricevuto"
        return $response.token
    } catch {
        throw "Login reale fallito su $BaseUrl/api/Auth/login. Stack NON funzionante: $($_.Exception.Message)"
    }
}

function Wait-HttpReady {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 | Out-Null
            Write-Host "HTTP ready: $Url"
            return
        } catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "Timeout avvio HTTP: $Url"
}

function Test-MissionReports {
    param(
        [string]$BaseUrl,
        [string]$Token
    )

    Write-Step "Verifica missioni Demo API"
    try {
        $headers = @{ Authorization = "Bearer $Token" }
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/mission/reports/index?runtimeMode=Demo&fromUtc=2016-01-01&toUtc=2026-12-31&skip=0&limit=5" -Headers $headers -TimeoutSec 20
        $total = $response.data.total
        Write-Host "Missioni Demo API: OK total=$total"

        $firstSessionId = @($response.data.items)[0].sessionId
        if ($firstSessionId) {
            $html = Invoke-WebRequest -Uri "$BaseUrl/api/mission/report/${firstSessionId}?format=html" -Headers $headers -UseBasicParsing -TimeoutSec 20
            if ($html.StatusCode -ne 200 -or $html.Content -notmatch "<html|<!doctype html") {
                throw "HTML missione non valido per sessionId=$firstSessionId"
            }
            Write-Host "Missione HTML: OK sessionId=$firstSessionId bytes=$($html.Content.Length)"
        }
    } catch {
        throw "Verifica missioni Demo fallita su $BaseUrl/api/mission/reports/index: $($_.Exception.Message)"
    }
}

function Invoke-Smoke {
    param([string]$BaseUrl)

    Write-Step "Smoke finale app locale"
    Wait-HttpReady -Url "http://localhost:5001" -TimeoutSeconds 30
    Wait-HttpReady -Url "$BaseUrl/api/Auth/test" -TimeoutSeconds 30
    $token = Test-RealLogin -BaseUrl $BaseUrl
    Test-MissionReports -BaseUrl $BaseUrl -Token $token
    Write-Host "Smoke finale: OK"
}

Push-Location $PSScriptRoot
try {
    Assert-Dash2Safety

    if ($SmokeOnly) {
        Invoke-Smoke -BaseUrl $ApiBaseUrl
        return
    }

    Write-Step "Stop pulito processi locali DASH2"
    @(5001, 5173, 5299, 7203, 5286, 7084) | ForEach-Object { Stop-Port $_ }

    if (-not $SkipBuild) {
        Write-Step "Clean e build backend WebApi"
        dotnet clean "backend/WebApi/WebApi.csproj"
        dotnet build "backend/WebApi/WebApi.csproj"

        Write-Step "Build frontend"
        Push-Location "frontend"
        try {
            npm run build
        } finally {
            Pop-Location
        }
    }

    if ($Run) {
        Assert-LocalFrontendApi
        Write-Step "Avvio WebApi e frontend locali"

        $backendCommand = "cd `"$PSScriptRoot\backend\WebApi`"; "
        if ($UseLocalDb -and $ApiBaseUrl -match "localhost:5299|127\.0\.0\.1:5299") {
            $connectionString = "Server=(localdb)\MSSQLLocalDB;Database=$LocalDbName;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
            $backendCommand += "`$env:ASPNETCORE_ENVIRONMENT='$AspNetCoreEnvironment'; `$env:ConnectionStrings__DefaultConnection='$connectionString'; `$env:Database__EnsureCreated='true'; "
            Write-Host "Backend DB locale: $LocalDbName (ASPNETCORE_ENVIRONMENT=$AspNetCoreEnvironment)"
            Write-Host "Decider remoto: http://51.178.16.37 (config Decider in appsettings.LocalProdLike.json)"
        }
        $backendCommand += "dotnet run --launch-profile LocalProdLike"

        Start-Process powershell -ArgumentList "-NoExit", "-Command", $backendCommand
        Wait-HttpReady -Url "$ApiBaseUrl/api/Auth/test" -TimeoutSeconds 90

        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\frontend`"; `$env:VITE_API_BASE_URL='$ApiBaseUrl'; npm run dev -- --host 0.0.0.0 --port 5001 --strictPort"
        Wait-HttpReady -Url "http://localhost:5001" -TimeoutSeconds 90
        Invoke-Smoke -BaseUrl $ApiBaseUrl
        Write-Host "WebApi: http://localhost:5299"
        Write-Host "Frontend: http://localhost:5001"
        Write-Host "Frontend API: $ApiBaseUrl"
    } else {
        Write-Host ""
        Write-Host "Restart safe completato. Usa -Run per avviare WebApi e frontend locali." -ForegroundColor Green
    }
} finally {
    Pop-Location
}
