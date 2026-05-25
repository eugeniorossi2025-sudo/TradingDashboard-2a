# File comandi repo

Comandi operativi standard per DASH2 / DASH2A.

Regole fisse:
- Eseguire questi comandi solo dal repo `TradingDashboard-2a`.
- Non usare `firebase deploy` senza ordine esplicito.
- Non usare `firebase use`.
- Non toccare Dashboard 1.
- Se un audit trova riferimenti Dashboard 1, backend rimossi, endpoint vecchi o fallback sporchi, fermarsi.
- Il frontend pubblico corretto e `https://eugenio-dashboard-2a.web.app/auth/login?redirect=/pages/user`.

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

URL attesi (stack locale-prod-like):
- Frontend: `http://localhost:5001` → WebApi locale `http://localhost:5299`
- WebApi → DB locale `Dash2A_LocalProdLike`
- WebApi → Decider `http://51.178.16.37` **solo** `/api/decider/config` e `/api/decider/health` (nessun sync verso DB)
- `Pc_CurrentStatus` dashboard: **DB locale**, non Decider live

Stato atteso:
1. Stack applicativo locale: OK
2. Decider remoto raggiungibile (health): OK
3. Dashboard dati live Decider: **NO** (non implementato)

Decisore produzione DASH2A (engine autonomo, VPS separato): `http://51.210.181.37` — non confondere con WebApi.
Gamebot legacy (Dashboard 1): `http://51.178.16.37` — stack separato, non usato dal frontend Vue DASH2A.

```powershell
powershell -ExecutionPolicy Bypass -File .\restart-app-safe.ps1 -Run
```

Per forzare un backend diverso:

```powershell
powershell -ExecutionPolicy Bypass -File .\restart-app-safe.ps1 -Run -ApiBaseUrl "http://51.83.159.175"
```

## Validazione reale obbligatoria

Non dire mai "funziona" solo perche la pagina apre o risponde `200`.

Prima di dichiarare lo stack locale funzionante verificare sempre:
- DevTools Network aperto;
- login reale eseguito da UI;
- chiamata `POST /api/Auth/login` verso API corretta;
- response auth `200` con token JWT reale;
- token salvato in storage client;
- chiamate API successive con `Authorization: Bearer ...`;
- SignalR verso `/dashboardHub` sulla URL corretta;
- websocket/SSE/long polling realmente connesso;
- `VITE_API_BASE_URL` effettivo coerente con ambiente locale/dev;
- `Dashboard.Url` / `Dashboard.UrlDev` coerenti con app.config Gamebot;
- nessuna chiamata nascosta a backend remoto o vecchi endpoint.

Stato minimo per dire "funziona":
- frontend `http://localhost:5001`;
- API effettiva controllata in Network;
- login admin reale OK;
- token reale presente;
- SignalR reale OK;
- Network senza chiamate a backend sbagliato.

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
Select-String -Path .\frontend\**\* -Pattern "dashboard-1|firebase\s+deploy|firebase\s+use|old endpoint|dirty fallback" -CaseSensitive:$false
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
