param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactPath,

    [string]$SiteName     = 'demoapp',
    [string]$AppPoolName  = 'demoapp',
    [string]$ReleaseRoot  = 'C:\inetpub\wwwroot\releases',
    [string]$SharedConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$SmokeTestUrl = 'http://localhost/api/Auth/test'
)

$ErrorActionPreference = 'Stop'

function Write-Step { param([string]$msg) Write-Host "==> $msg" }

Import-Module WebAdministration

$site    = Get-Website -Name $SiteName -ErrorAction Stop
$appPool = Get-Item "IIS:\AppPools\$AppPoolName" -ErrorAction Stop

$currentPath = $site.PhysicalPath
Write-Step "IIS: site=$($site.Name) | pool=$($appPool.Name) | current=$currentPath"

# Forbid DB migration tools
foreach ($tool in @('dotnet-ef.exe','ef.exe')) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) {
        Write-Host "DB migration tool present but NOT used: $tool"
    }
}

$timestamp   = Get-Date -Format 'yyyyMMdd-HHmmss'
$releasePath = Join-Path $ReleaseRoot "backend-$timestamp"

Write-Step "Copying artifact to release folder: $releasePath"
New-Item -ItemType Directory -Force -Path $releasePath | Out-Null
Get-ChildItem -LiteralPath $ArtifactPath -Force | Copy-Item -Destination $releasePath -Recurse -Force

Write-Step "Applying shared production config"
if (-not (Test-Path -LiteralPath $SharedConfigPath)) {
    throw "Shared production config not found: $SharedConfigPath"
}
Copy-Item -LiteralPath $SharedConfigPath -Destination (Join-Path $releasePath 'appsettings.Production.json') -Force

Write-Step "Setting ASPNETCORE_ENVIRONMENT=Production in release web.config"
$webConfigPath = Join-Path $releasePath 'web.config'
if (-not (Test-Path -LiteralPath $webConfigPath)) {
    throw "web.config not found in release path: $webConfigPath"
}

[xml]$webConfig = Get-Content -LiteralPath $webConfigPath -Raw
$systemWebServer = $webConfig.configuration.'system.webServer'
if ($null -eq $systemWebServer) {
    $systemWebServer = $webConfig.CreateElement('system.webServer')
    [void]$webConfig.configuration.AppendChild($systemWebServer)
}

$aspNetCore = $systemWebServer.aspNetCore
if ($null -eq $aspNetCore) {
    throw "aspNetCore element not found in web.config: $webConfigPath"
}

$environmentVariables = $aspNetCore.environmentVariables
if ($null -eq $environmentVariables) {
    $environmentVariables = $webConfig.CreateElement('environmentVariables')
    [void]$aspNetCore.AppendChild($environmentVariables)
}

$envVar = $environmentVariables.environmentVariable | Where-Object { $_.name -eq 'ASPNETCORE_ENVIRONMENT' } | Select-Object -First 1
if ($null -eq $envVar) {
    $envVar = $webConfig.CreateElement('environmentVariable')
    $envVar.SetAttribute('name', 'ASPNETCORE_ENVIRONMENT')
    [void]$environmentVariables.AppendChild($envVar)
}
$envVar.SetAttribute('value', 'Production')
$webConfig.Save($webConfigPath)

Write-Step "Stopping app pool: $AppPoolName"
Stop-WebAppPool -Name $AppPoolName
$waited = 0
do {
    Start-Sleep -Seconds 2; $waited += 2
    $state = (Get-WebAppPoolState -Name $AppPoolName).Value
} while ($state -ne 'Stopped' -and $waited -lt 30)
if ($state -ne 'Stopped') { throw "App pool did not stop within 30s (state: $state)" }
Write-Host "App pool stopped after ${waited}s"

try {
    Write-Step "Swapping IIS PhysicalPath to: $releasePath"
    Set-WebConfigurationProperty `
        -Filter "system.applicationHost/sites/site[@name='$SiteName']/application[@path='/']/virtualDirectory[@path='/']" `
        -Name "physicalPath" `
        -Value $releasePath

    Write-Step "Starting app pool: $AppPoolName"
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 10

    Write-Step "Smoke test: $SmokeTestUrl"
    $response = Invoke-WebRequest -Uri $SmokeTestUrl -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -ne 200) {
        throw "Smoke test failed: HTTP $($response.StatusCode)"
    }

    Write-Step "Deploy OK"
    Write-Host "RELEASE_PATH=$releasePath"
    Write-Host "PREVIOUS_PATH=$currentPath"
    Write-Host "IIS_SITE=$SiteName"
    Write-Host "SMOKE_STATUS=$($response.StatusCode)"
}
catch {
    Write-Host "DEPLOY_FAILED=$($_.Exception.Message)"
    Write-Step "Rollback: ripristino PhysicalPath a $currentPath"

    Set-WebConfigurationProperty `
        -Filter "system.applicationHost/sites/site[@name='$SiteName']/application[@path='/']/virtualDirectory[@path='/']" `
        -Name "physicalPath" `
        -Value $currentPath

    Start-WebAppPool -Name $AppPoolName
    Write-Host "Rollback completato - sito ripristinato su: $currentPath"
    throw
}
