# File comandi repo

Comandi operativi standard per DASH2 / DASH2A.

Regole fisse:
- Eseguire questi comandi solo dal repo `TradingDashboard-2a`.
- Non usare `firebase deploy` senza ordine esplicito.
- Non usare `firebase use`.
- Non toccare Dashboard 1.
- Se un audit trova `eugenio-dashboard-1`, `dashboard-1` o `eugenio-dashboard-2a`, fermarsi.

## Restart APP

Comando standard da usare quando viene richiesto "Restart APP".

Fa:
- verifica repo DASH2A;
- verifica Firebase `eugenio-dashboard-2`;
- blocca riferimenti Dashboard 1 / project errato;
- stop pulito porte locali;
- clean e build backend;
- build frontend.

```powershell
powershell -ExecutionPolicy Bypass -File .\restart-app-safe.ps1
```

## Restart APP e avvio locale

Come `Restart APP`, ma alla fine avvia anche WebApi e frontend locali.

URL attesi:
- WebApi: `http://localhost:5299`
- Frontend: `http://localhost:5173`

```powershell
powershell -ExecutionPolicy Bypass -File .\restart-app-safe.ps1 -Run
```

## Restart APP rapido senza build

Ferma le porte locali e verifica sicurezza, ma salta build.
Usare solo quando la build e gia stata fatta.

```powershell
powershell -ExecutionPolicy Bypass -File .\restart-app-safe.ps1 -SkipBuild
```

## Verifica repo e stato

Controlla branch, remote e modifiche locali.

```powershell
git remote -v
git branch --show-current
git status --short
```

## Audit Firebase sicuro

Controlla che Firebase sia Dashboard 2 e che non ci siano riferimenti pericolosi.

```powershell
Get-Content .\frontend\.firebaserc
Get-Content .\frontend\firebase.json
Select-String -Path .\frontend\**\* -Pattern "eugenio-dashboard-1|dashboard-1|eugenio-dashboard-2a|firebase\s+deploy|firebase\s+use" -CaseSensitive:$false
```

## Build backend

Build locale WebApi DASH2A.

```powershell
dotnet build .\backend\WebApi\WebApi.csproj
```

## Clean build backend

Pulizia e build locale WebApi DASH2A.

```powershell
dotnet clean .\backend\WebApi\WebApi.csproj
dotnet build .\backend\WebApi\WebApi.csproj
```

## Build frontend

Build locale frontend DASH2A.

```powershell
cd .\frontend
npm run build
cd ..
```

## Lint mirato file patch

Usare per controllare i file Vue/JS modificati senza scansionare tutta la repo.

```powershell
cd .\frontend
cmd /c "set ESLINT_USE_FLAT_CONFIG=false&& npx eslint --fix src/layout/AppMenu.vue src/router/index.js src/views/Dashboard.vue src/views/client/ClientDesktop.vue src/views/mobile/AdminMobileLive.vue src/views/mobile/ClientMobile.vue src/views/pages/Configuration.vue src/views/pages/Log.vue src/views/pages/auth/Login.vue public/service-worker.js --ext .vue,.js,.jsx,.cjs,.mjs"
cd ..
```

## Commit locale

Controllare prima lo stato, poi committare solo file pertinenti.

```powershell
git status --short
git diff --cached --stat
git commit -m "messaggio chiaro"
```

## Ultimi commit

Mostra gli ultimi commit locali.

```powershell
git log -5 --oneline
```
