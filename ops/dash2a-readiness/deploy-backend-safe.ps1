param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactPath,

    [string]$SiteName     = 'demoapp',
    [string]$AppPoolName  = 'demoapp',
    [string]$ReleaseRoot  = 'C:\inetpub\wwwroot\releases',
    [string]$SharedConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$SmokeTestUrl = 'http://127.0.0.1/api/Auth/test',
    [string]$CollaudoMirrorSecret = ''
)

$ErrorActionPreference = 'Stop'

function Set-IisAppPoolEnvVar {
    param(
        [string]$AppPoolName,
        [string]$Name,
        [string]$Value
    )
    $filter = "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables"
    $existing = Get-WebConfiguration -Filter "$filter/add[@name='$Name']" -ErrorAction SilentlyContinue
    if ($existing) {
        Set-WebConfigurationProperty -Filter "$filter/add[@name='$Name']" -Name "value" -Value $Value
    }
    else {
        Add-WebConfigurationProperty -Filter $filter -Name "." -Value @{ name = $Name; value = $Value }
    }
}

function Invoke-LocalSmokeTest {
    param([string]$Url)

    $previousCallback = [System.Net.ServicePointManager]::ServerCertificateValidationCallback
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

    try {
        try {
            return Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30
        }
        catch {
            if ($Url -notmatch '^https://') {
                $httpsUrl = $Url -replace '^http://', 'https://'
                Write-Host "Smoke HTTPS fallback: $httpsUrl"
                return Invoke-WebRequest -Uri $httpsUrl -UseBasicParsing -TimeoutSec 30
            }
            throw
        }
    }
    finally {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $previousCallback
    }
}

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

    Write-Step "Setting app pool environment: ASPNETCORE_ENVIRONMENT=Production"
    $envVarFilter = "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables/add[@name='ASPNETCORE_ENVIRONMENT']"
    $existingEnvVar = Get-WebConfigurationProperty -Filter $envVarFilter -Name "value" -ErrorAction SilentlyContinue
    if ($null -eq $existingEnvVar) {
        Add-WebConfigurationProperty `
            -Filter "system.applicationHost/applicationPools/add[@name='$AppPoolName']/environmentVariables" `
            -Name "." `
            -Value @{ name = "ASPNETCORE_ENVIRONMENT"; value = "Production" }
    }
    else {
        Set-WebConfigurationProperty `
            -Filter $envVarFilter `
            -Name "value" `
            -Value "Production"
    }

    if ($CollaudoMirrorSecret) {
        Write-Step "Setting app pool env: Collaudo__MirrorSecret (from GitHub secret)"
        Set-IisAppPoolEnvVar -AppPoolName $AppPoolName -Name 'Collaudo__MirrorSecret' -Value $CollaudoMirrorSecret
    }

    Write-Step "Starting app pool: $AppPoolName"
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 10

    Write-Step "Smoke test: $SmokeTestUrl"
    $response = Invoke-LocalSmokeTest -Url $SmokeTestUrl
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
