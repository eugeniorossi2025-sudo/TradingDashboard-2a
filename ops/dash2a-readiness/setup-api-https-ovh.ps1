# Issue and bind a certificate for the current DASH2A API IIS site.
$ErrorActionPreference = 'Stop'
if ($env:COMPUTERNAME -ne 'WIN-L28URC6KJHG') { throw 'Wrong server' }
$principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Administrator PowerShell required' }
Import-Module WebAdministration
$site = Get-Website -Name 'DASH2A-API'
if (-not $site) { throw 'DASH2A-API site missing' }
$hostName = 'api.tradingdash2a.com'
$http = @(Get-WebBinding -Name $site.Name -Protocol http | Where-Object { $_.bindingInformation -eq ('*:80:' + $hostName) })
if ($http.Count -ne 1) { throw 'Expected API HTTP binding absent or ambiguous' }
$existingHttps = @(Get-WebBinding -Name $site.Name -Protocol https -ErrorAction SilentlyContinue | Where-Object { $_.bindingInformation -like ('*:443:' + $hostName) })
if ($existingHttps.Count) { throw 'HTTPS binding already exists; inspect it before issuing another certificate' }
$zip = Join-Path $env:TEMP 'win-acme.v2.2.9.1701.x64.trimmed.zip'
$uri = 'https://github.com/win-acme/win-acme/releases/download/v2.2.9.1701/win-acme.v2.2.9.1701.x64.trimmed.zip'
Invoke-WebRequest -Uri $uri -OutFile $zip
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
if ($hash -ne 'F4DC3B144841FFDBA391CE168C273D7A686D45A359075E30EE4BF4EE186857D6') { throw 'win-acme archive hash mismatch' }
$folder = 'C:\ProgramData\win-acme\v2.2.9.1701'
if (Test-Path -LiteralPath $folder) { throw 'win-acme folder already exists; inspect before repeating' }
New-Item -ItemType Directory -Path $folder -Force | Out-Null
Expand-Archive -LiteralPath $zip -DestinationPath $folder
$wacs = Join-Path $folder 'wacs.exe'
if (-not (Test-Path -LiteralPath $wacs)) { throw 'wacs.exe not found in verified archive' }
& $wacs --source iis --siteid $site.Id --host $hostName --validationmode http-01 --validation selfhosting --installation iis --installationsiteid $site.Id --accepttos
if ($LASTEXITCODE -ne 0) { throw "Certificate issuance failed with exit code $LASTEXITCODE" }
$https = @(Get-WebBinding -Name $site.Name -Protocol https | Where-Object { $_.bindingInformation -like ('*:443:' + $hostName) })
if ($https.Count -ne 1) { throw 'Certificate tool exited successfully but API HTTPS binding is missing' }
$rule = Get-NetFirewallRule -Name 'DASH2A-API-HTTPS-443' -ErrorAction SilentlyContinue
if (-not $rule) {
  New-NetFirewallRule -Name 'DASH2A-API-HTTPS-443' -DisplayName 'DASH2A API HTTPS 443' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 443 -Profile Any | Out-Null
}
Write-Host 'HTTPS_SETUP_PASS: certificate bound to DASH2A-API; Windows inbound TCP 443 allowed.'
