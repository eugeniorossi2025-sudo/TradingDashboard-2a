# Dash2A — deploy frontend Firebase (live)
# Prerequisito: eseguire una volta `firebase login` nello stesso terminale/utente Windows.

$ErrorActionPreference = 'Stop'
$Root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Frontend = Join-Path $Root 'frontend'

if (-not (Get-Command firebase -ErrorAction SilentlyContinue)) {
  throw 'Firebase CLI non trovato. Installa: npm i -g firebase-tools'
}

Push-Location $Frontend
try {
  firebase projects:list 2>$null | Out-Null
  if ($LASTEXITCODE -ne 0) {
    throw @'
Firebase non autenticato.
Esegui prima (si apre il browser, login manuale ~30s):
  firebase login
Poi rilancia questo script.
'@
  }

  $env:VITE_API_BASE_URL = 'https://vps-b0942869.vps.ovh.net'
  Write-Host '>> npm ci' -ForegroundColor Cyan
  npm ci
  Write-Host '>> npm run build' -ForegroundColor Cyan
  npm run build
  Write-Host '>> firebase deploy --only hosting --project eugenio-dashboard-2a' -ForegroundColor Cyan
  firebase deploy --only hosting --project eugenio-dashboard-2a
  Write-Host 'OK: https://eugenio-dashboard-2a.web.app/' -ForegroundColor Green
}
finally {
  Pop-Location
}
