param(
    [switch]$Run,
    [switch]$SkipBuild,
    [string]$ApiBaseUrl = "http://51.83.159.175"
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
    } catch {
        throw "Login reale fallito su $BaseUrl/api/Auth/login. Stack NON funzionante: $($_.Exception.Message)"
    }
}

Push-Location $PSScriptRoot
try {
    Assert-Dash2Safety

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
        Test-RealLogin -BaseUrl $ApiBaseUrl
        Write-Step "Avvio WebApi e frontend locali"
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\backend\WebApi`"; dotnet run --launch-profile http"
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\frontend`"; `$env:VITE_API_BASE_URL='$ApiBaseUrl'; npm run dev -- --host 0.0.0.0 --port 5001 --strictPort"
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
