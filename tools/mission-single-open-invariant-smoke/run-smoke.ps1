# Mission single-open invariant: logic smoke + WebApi compile check.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "==> Logic smoke"
dotnet run --project (Join-Path $PSScriptRoot "MissionSingleOpenInvariantSmoke.csproj") -c Release

Write-Host "==> WebApi build (patch compiles)"
dotnet build (Join-Path $root "backend\WebApi\WebApi.csproj") -c Release --no-restore 2>$null
if ($LASTEXITCODE -ne 0) {
    dotnet restore (Join-Path $root "backend\WebApi\WebApi.csproj")
    dotnet build (Join-Path $root "backend\WebApi\WebApi.csproj") -c Release
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "OK: mission accounting invariant smoke passed."
