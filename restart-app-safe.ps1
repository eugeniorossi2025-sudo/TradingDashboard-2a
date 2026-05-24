param(
    [switch]$Run,
    [switch]$SkipBuild
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

    $dangerPattern = "eugenio-dashboard-1|dashboard-1|eugenio-dashboard-2a|firebase\s+deploy|firebase\s+use"
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

Push-Location $PSScriptRoot
try {
    Assert-Dash2Safety

    Write-Step "Stop pulito processi locali DASH2"
    @(5173, 5299, 7203, 5286, 7084) | ForEach-Object { Stop-Port $_ }

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
        Write-Step "Avvio WebApi e frontend locali"
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\backend\WebApi`"; dotnet run --launch-profile http"
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\frontend`"; npm run dev -- --host 0.0.0.0"
        Write-Host "WebApi: http://localhost:5299"
        Write-Host "Frontend: http://localhost:5173"
    } else {
        Write-Host ""
        Write-Host "Restart safe completato. Usa -Run per avviare WebApi e frontend locali." -ForegroundColor Green
    }
} finally {
    Pop-Location
}
