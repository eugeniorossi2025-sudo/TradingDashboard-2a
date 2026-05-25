# Abilita HTTPS IIS per WebApi DASH2A su 51.83.159.175
# Hostname OVH: vps-b0942869.vps.ovh.net (Let's Encrypt valido)
# NON tocca DB. NON deploy frontend.

param(
    [string]$SiteName = 'demoapp',
    [string]$Hostname = 'vps-b0942869.vps.ovh.net',
    [string]$ContactEmail = 'ak47129898@gmail.com',
    [string]$WacsDir = 'C:\tools\win-acme',
    [string]$SmokePath = '/api/Auth/test'
)

$ErrorActionPreference = 'Stop'
Import-Module WebAdministration

Write-Host "==> Firewall 443"
$rule = Get-NetFirewallRule -DisplayName 'DASH2A-HTTPS-443' -ErrorAction SilentlyContinue
if (-not $rule) {
    New-NetFirewallRule -DisplayName 'DASH2A-HTTPS-443' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 443 | Out-Null
}

Write-Host "==> win-acme (Let's Encrypt)"
New-Item -ItemType Directory -Force -Path $WacsDir | Out-Null
$wacs = Join-Path $WacsDir 'wacs.exe'
if (-not (Test-Path $wacs)) {
    $zip = Join-Path $env:TEMP 'win-acme.zip'
    Invoke-WebRequest -Uri 'https://github.com/win-acme/win-acme/releases/download/v2.2.9.1701/win-acme.v2.2.9.1701.x64.pluggable.zip' -OutFile $zip
    Expand-Archive -Path $zip -DestinationPath $WacsDir -Force
}

$site = Get-Website -Name $SiteName -ErrorAction Stop
Write-Host "==> IIS site $($site.Name) id=$($site.Id) hostname=$Hostname"

& $wacs `
    --source iis `
    --siteid $site.Id `
    --validation iis `
    --validationsiteid $site.Id `
    --installation iis `
    --installationsiteid $site.Id `
    --accepttos `
    --emailaddress $ContactEmail `
    --notaskscheduler

$smoke = "https://${Hostname}${SmokePath}"
Write-Host "==> Smoke: $smoke"
$r = Invoke-WebRequest -Uri $smoke -UseBasicParsing -TimeoutSec 30 -SkipCertificateCheck:$false
Write-Host "Smoke status: $($r.StatusCode)"
