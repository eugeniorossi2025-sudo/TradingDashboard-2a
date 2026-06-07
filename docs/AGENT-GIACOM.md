# AGENT-GIACOM — Dashboard DASH2A (documento unico)

> **Agente:** `giacom` / `/giacom`
> **Scope:** Vue frontend, WebApi, Decisore, deploy DASH2A — **NON** GameBot (`giacomo1`), **NON** Dashboard 1 / Eugenio (`marco`, `marco1`)
> **Marker:** `agent-giacom:v2026-06-06`
> **Leggere SOLO questo file** — non aprire altri doc DASH2A salvo task esplicito dell'utente.
> **Aggiornare a fine scope:** sezione **`Stato sessione`** in **questo file** (non altri `AGENT-*`, non `AI-STATE.md`).

---

## Identità e separazione obbligatoria

| | **giacom (questo agente)** | **giacomo1 (altro agente)** | **marco / marco1 (Dashboard 1)** |
|--|---------------------------|----------------------------|----------------------------------|
| Repo | `NuovaDashboard-MarcoTurri` | stesso repo, solo bot | `TradingDashboard-iis` |
| Bot sorgente | **NON toccare** `tools/eugenio-bot` | `tools/eugenio-bot` | `baccarat-bot-main-socket-artifact` |
| Doc | **questo file** | `AGENT-GIACOMO1.md` | `AGENT-MARCO.md` / `AGENT-MARCO1.md` |

**Clone obbligatorio:** `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`
**Remote:** `eugeniorossi2025-sudo/TradingDashboard-2a`
**Branch:** `main`

**Documento da aggiornare a fine scope:** `docs/AGENT-GIACOM.md` → sezione **`Stato sessione`** (sotto).

---

## Stato sessione (agente aggiorna a fine ogni scope)

> Sovrascrivere **solo questo blocco** quando il task è concluso o l'utente scrive **`chiudi scope`**.

**Marker sessione:** `agent-giacom:session:2026-06-06`

### Ultimo tema
- Documentazione agenti DASH2A: `AGENT-GIACOM.md` / `AGENT-GIACOMO1.md` + regola chiusura scope.

### Fatto
- Push `main` commit `ada13cf` — doc agenti giacom/giacomo1 su GitHub.
- Stack prod allineato (2026-06-05): WebApi + Firebase + Decisore @ `8e2efe1`.

### Resta / prossimo passo
- Lavori dashboard/Decisore/WebApi secondo task utente; bot Giacomo → agente **giacomo1**.

### Deploy / commit
- Doc: `ada13cf` su `main` (TradingDashboard-2a).
- Smoke: `GET https://vps-b0942869.vps.ovh.net/api/Auth/test` → 200; frontend `https://eugenio-dashboard-2a.web.app/`.

---

## PARTE A — Workspace Guard (checklist git obbligatoria)

# DASH2A — Workspace Guard (obbligatorio)

> **Leggere questo file prima di ogni audit, patch, commit, validazione pre-deploy o attività agent su DASH2.**
> Se una condizione non è soddisfatta: **fermarsi subito** e non modificare nulla.

---

## 1. Repository corretto

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

URL GitHub:

```text
https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git
```

---

## 2. Clone locale corretto

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

---

## 3. Branch operativo

```text
main
```

---

## 4. Regola assoluta

**Qualsiasi attività su DASH2 deve partire da questo path:**

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

Non aprire, non patchare e non committare da altri workspace Cursor.

---

## 5. Checklist obbligatoria (prima di ogni audit / patch / commit)

Eseguire **sempre** dalla root del clone DASH2A:

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
git remote -v
git branch --show-current
git status
git rev-parse --show-toplevel
```

Verificare che l'output sia coerente con le sezioni 1–3 di questo file.

---

## 6. Stop immediato se il path non è quello corretto

Se `git rev-parse --show-toplevel` **non** restituisce esattamente:

```text
C:/Users/eugen/Desktop/NuovaDashboard-MarcoTurri
```

(o l'equivalente con backslash `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`)

→ **fermarsi subito. Non modificare nulla.**

---

## 7. Stop immediato se il remote non è quello corretto

Se `git remote -v` **non** contiene:

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

→ **fermarsi subito. Non modificare nulla.**

Remote tipici **vietati** per lavori DASH2:

- `TradingDashboard-iis`
- repo / path legati a Dashboard 1
- `PCTEST45\TradingDashboard`
- qualsiasi clone diverso da `NuovaDashboard-MarcoTurri`

---

## 8. Dashboard 1 — fuori scope

Dashboard 1 (**TradingDashboard-iis**, `PCTEST45\TradingDashboard`, Eugenio-Demo10, Firebase Dashboard 1, VPS legacy IIS) è **fuori scope** per DASH2.

| Azione | Consentita per DASH2? |
|--------|----------------------|
| Leggere codice Dashboard 1 | **No** |
| Patchare Dashboard 1 | **No** |
| Committare su Dashboard 1 per lavori DASH2 | **No** |
| Usare credenziali / DB / URL Dashboard 1 | **No** |

Per DASH2 usare **solo** il clone e il remote indicati in questo documento.

---

## Riferimenti correlati

- Infrastruttura completa: [`DASH2A-INFRASTRUCTURE.md`](../../DASH2A-INFRASTRUCTURE.md) (root repo)
- Report contabilità: [`REPORT-CONTABILITA-CANONICA.md`](./REPORT-CONTABILITA-CANONICA.md)
- Validazione report: [`validate-report-coherence.ps1`](./validate-report-coherence.ps1)

---

*Documento operativo DASH2A — aggiornato 2026-05-30*


---

## PARTE B — Documenti canonici report log

# Documenti canonici — solo DASH2A / Giacomo

> **Scope di questo file:** repository `NuovaDashboard-MarcoTurri` (DASH2A).  
> **Non toccare** documentazione Eugenio / Dashboard 1 (`EUGENIO13`, `EUGENIO11-ARCHITECTURE.md`, repo `TradingDashboard`) — è un altro sistema.  
> **Ultimo aggiornamento:** 2026-06-06

---

## Regola: un report = un documento

Per analisi log produzione DASH2A, indice completo: [`tools/prod-log-export/REPORT-LOG-PROD.md`](tools/prod-log-export/REPORT-LOG-PROD.md)

| Report | Documento canonico | PDF (2026-06-06) |
|--------|-------------------|------------------|
| P&L per zona mazzo (10 gg prod) | `tools/prod-log-export/deck_zone_analysis/deck_zone_prod_report.md` | `Desktop\DASH2A_Analisi_Zona_Mazzo_COMPLETO_2026-06-06.pdf` |
| Simulazione L5 bloccati | `tools/prod-log-export/l5_blocked_analysis/l5_blocked_prod_report.md` | `Desktop\DASH2A_L5_Bloccati_Simulazione_2026-06-06.pdf` |
| Filtro L5 per zona mazzo | `tools/prod-log-export/l5_blocked_analysis/l5_zone_strategy_report.md` | `Desktop\DASH2A_Filtro_L5_Zona_Mazzo_2026-06-06.pdf` |

**Obsoleto (stesso report, campione vecchio):** `deck_zone_analysis/deck_zone_48h_report.md` → usare `deck_zone_prod_report.md`.

---

## Infrastruttura DASH2A

| Argomento | Leggere SOLO |
|-----------|----------------|
| VPS, deploy, WebApi, Decisore, credenziali | `DASH2A-INFRASTRUCTURE.md` |
| GameBot **Giacomo** (sorgente, release, runtime) | `DASH2A-INFRASTRUCTURE.md` **§14** |
| Checklist git obbligatoria | `ops/dash2a-readiness/DASH2A-WORKSPACE-GUARD.md` |

**Non leggere per DASH2A:** `tools/eugenio-gamebot/README.md` (solo redirect), `ops/archive/*`, audit generici — salvo task esplicito.

---

## Separazione obbligatoria — due dashboard

| | **DASH2A / Giacomo** | **Dashboard 1 / Eugenio** |
|--|----------------------|---------------------------|
| Repo | `NuovaDashboard-MarcoTurri` | `TradingDashboard` (IIS) |
| Bot sorgente | `tools/eugenio-bot` — agente **`giacomo1`** | `baccarat-bot-main-socket-artifact` — agente **`marco1`** |
| Runtime tipico | `relisegiacomo ok 1.7` (linea giacomo1) | `EUGENIO13` |
| Doc infra | `DASH2A-INFRASTRUCTURE.md` — agente **`giacom`** | `docs/EUGENIO13-GAMEBOT.md` |

**Non modificare, unificare o marcare obsoleti** i file Eugenio da lavoro DASH2A.


---

## PARTE C — Infrastruttura DASH2A (§1–§13, §15 — senza §14 GameBot)

> La sezione **§14 GameBot** è **esclusa** da questo documento. Per bot Giacomo usare agente **giacomo1** e `AGENT-GIACOMO1.md`.
# DASH2A — Infrastruttura Definitiva

> **Leggere questo file all'inizio di ogni sessione DASH2A.**
> **Report analisi log (un doc per report):** [`DOCS-CANONICI.md`](DOCS-CANONICI.md) — solo scope DASH2A, non toccare doc Eugenio/Dashboard 1.
> **Prima di qualsiasi attività:** leggere [`ops/dash2a-readiness/DASH2A-WORKSPACE-GUARD.md`](ops/dash2a-readiness/DASH2A-WORKSPACE-GUARD.md) ed eseguire la checklist git obbligatoria.
> Aggiornare quando cambiano IP, credenziali, o configurazioni.
> **Ultimo aggiornamento: 2026-06-06** — §14 GameBot **1.6** (waiting probe mani 3/6/9, uscita a mano 9, preset contatore PAUSE=1) su `tools/eugenio-bot`; runtime release attiva **`relisegiacomo ok 1.6`**. **Documento padre GameBot:** solo questo file §14 — non duplicare altrove. Repo `eugeniorossi2025-sudo/TradingDashboard-2a`, clone `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`, branch prod `main`.

---

## 1. SISTEMI COINVOLTI — NON MESCOLARE

| Sistema | Repo | Scopo |
|---|---|---|
| **Dashboard 1 / IIS legacy** | `PCTEST45\TradingDashboard` / repo `TradingDashboard-iis` | Sistema legacy — bot / Gamebot storico; **non usare per patch DASH2A** |
| **DASH2A** | `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri` / repo `TradingDashboard-2a` | Nuovo sistema — Decisore + WebApi + frontend Vue |

**Regola assoluta:** credenziali, IP Firebase, e DB di Dashboard 1 non si usano per deploy DASH2A e viceversa.

**Controllo obbligatorio anti-errore prima di qualunque modifica DASH2A:**

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
git remote -v
```

Il remote deve essere:

```text
origin  https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git
```

Se il remote mostra `TradingDashboard-iis`, `PCTEST45\TradingDashboard`, Dashboard 1 o altri repo, **fermarsi**: si sta leggendo/modificando il workspace sbagliato.

> **Nota IP condiviso:** `51.178.16.37` compare nello stack legacy Dashboard 1 **e** come host attivo del **Decisore Proattivo** DASH2A (`/api/proactive`). Non confondere i due prodotti: Vue/WebApi DASH2A in produzione puntano a `51.83.159.175`, non al Decisore per i dati dashboard.

---

## 2. MAPPA VPS (validata 2026-05-25)

| Ruolo | Hostname OVH | IPv4 | Stato |
|---|---|---|---|
| **WebApi + SQL dashboard DASH2A** | `vps-b0942869.vps.ovh.net` | `51.83.159.175` | **Attivo** — IIS, SQL, runner CI |
| **Decisore Proattivo DASH2A** | `vps-4ca306e8.vps.ovh.net` | `51.178.16.37` | **Attivo** — `/api/proactive/*` → HTTP 200 |
| ~~Decisore vecchio~~ | `vps-138a2a47.vps.ovh.net` | `51.210.181.37` | **Obsoleto** — `/api/proactive/*` → 404; non usare |

### 2.1 VPS Backend Dashboard (OVH)

| Parametro | Valore |
|---|---|
| Provider | OVH |
| Hostname | `vps-b0942869.vps.ovh.net` |
| IPv4 | `51.83.159.175` |
| OS | Windows Server 2025 Standard (Desktop) — *validato via RDP/inventory* |
| SQL Server | Microsoft SQL Server 2025 (RTM) — *validato login `sa3` 2026-05-25* |
| Piano | VPS-2, 6 vCore, 12 GB RAM, 100 GB |
| Web Server | IIS 10 — *validato header `Microsoft-IIS/10.0`* |
| App Pool | `demoapp` |
| Publish path | `C:\inetpub\wwwroot\publish` |
| Release root | `C:\inetpub\wwwroot\releases` |
| Backup root | `C:\inetpub\wwwroot\backups` |
| RDP user | `administrator` |
| RDP password | GitHub Secret `DASH2A_RDP_PASSWORD` |
| Porte verificate | **80/443** HTTP/HTTPS (WebApi OK), **1434** SQL runtime, **3389** RDP |
| HTTPS | Porta **443 attiva** — Let's Encrypt su `vps-b0942869.vps.ovh.net` |
| Smoke test HTTP | `GET http://51.83.159.175/api/Auth/test` → **200** |
| Smoke test HTTPS | `GET https://vps-b0942869.vps.ovh.net/api/Auth/test` → **200** |

**Architettura dati produzione:** WebApi dashboard e Decisore usano lo stesso DB operativo su `51.83.159.175,1434` / `Eugenio-Demo10`. La UI Firebase non accede mai direttamente al DB: chiama la WebApi HTTPS, che in produzione carica `appsettings.Production.json` con `DefaultConnection` su `1434`.

### 2.2 VPS Decisore (OVH — engine separato)

| Parametro | Valore |
|---|---|
| Hostname | `vps-4ca306e8.vps.ovh.net` |
| IPv4 | **`51.178.16.37`** |
| Nome servizio | **Decisore Proattivo / Logica Multi-tavolo** |
| API runtime | `http://51.178.16.37/api/proactive` |
| Web Server | IIS 10 |
| IIS site | `default` |
| IIS app pool | **`Proactive`** (state: Started) |
| Path produzione attivo | `C:\Decisore` |
| Repo path sorgente | `decision-engine/Decisore/` |
| DB engine | **`51.83.159.175,1434`** (SQLEXPRESS01 sul VPS backend) / `Eugenio-Demo10` / login `sa3` |
| appsettings.json live | `Server=51.83.159.175,1434;Database=Eugenio-Demo10;...` — **NON modificare** |
| Deploy Decisore | GitHub Actions → `deploy-decisore-safe.yml` (`workflow_dispatch`, runner `DASH2A-DECISORE`) |
| Backup Decisore | `C:\DecisoreBackups\decisore-YYYYMMDD-HHmmss` |
| Stato 2026-05-27 | **OPERATIVO** — IIS `default`, app pool `Proactive`, binario deployato da workflow safe |

> **ATTENZIONE:** il Decisore usa il DB sul **VPS backend** (`51.83.159.175,1434`), NON un DB locale a se stesso. La WebApi live usa la stessa porta `1434` tramite `appsettings.Production.json`.

> La regola firewall `SQLEXPRESS01-1434-Decisore` sul VPS backend permette l'accesso dalla sola IP `51.178.16.37` porta 1434. Non rimuovere questa regola.

#### Route disponibili Decisore

| Endpoint | Metodo | Descrizione |
|---|---|---|
| `/api/proactive/reset` | GET | Reset sessione + svuota Pc_CurrentStatus (side effect!) |
| `/api/proactive/decide` | GET | Endpoint principale bot — richiede parametri query |
| `/api/proactive/emergency-stop` | GET | Stop d'emergenza tutti i bot attivi |
| `/api/proactive/update-params` | POST form | Aggiorna stato PC senza decide |
| `/api/proactive/update-deck` | POST form | Aggiorna solo mazzo |
| `/api/proactive/get-global-profit` | POST form | Legge margine/saldo da DB |
| `/api/proactive/bot-app-config` | POST JSON | Salva config bot su DB |

> **Non esiste `/health`** — `/api/proactive/health` restituisce 404 anche con app viva. Usare TCP 80 + inventory IIS come smoke neutro; usare `/reset` solo per test manuali perché ha side effect: svuota `Pc_CurrentStatus`.

**Locale:** la WebApi in `LocalProdLike` espone `/api/decider/config` e `/api/decider/health` come **sonda diagnostica** verso `51.178.16.37`. **Non** sincronizza `Pc_CurrentStatus` né alimenta la dashboard locale.

### 2.3 VPS obsoleta (solo riferimento storico)

| Parametro | Valore |
|---|---|
| Hostname | `vps-138a2a47.vps.ovh.net` |
| IPv4 | ~~`51.210.181.37`~~ |
| Stato | **Non usare** — probe `/api/proactive/reset` → 404 (2026-05-25) |
| Note | Sostituita da `51.178.16.37` per API proactive; rimuovere da config nuove |

---

## 3. DATABASE PRODUZIONE (runtime unico WebApi + Decisore)

| Parametro | Valore |
|---|---|
| Host | `51.83.159.175,1434` |
| Database | `Eugenio-Demo10` |
| Login | `sa3` |
| Password | **Secret / live config** (`appsettings.Production.json`, GitHub Secrets) — non documentare in chiaro |
| Encrypt | False |
| Runtime verificato | **OK** — `DASH2A Runtime Config Readonly Diagnostic` 2026-05-26: `SQL_CONFIG_SERVER=51.83.159.175,1434` |
| Config live | `C:\inetpub\wwwroot\shared\appsettings.Production.json` e release corrente allineate |
| EF Migrations | **NON eseguire** — `__EFMigrationsHistory` vuota per design |

> `appsettings.json` base nei release può ancora contenere `1433`: non usarlo come fonte runtime. In `ASPNETCORE_ENVIRONMENT=Production` vale `appsettings.Production.json`, che punta a `1434`.

### Tabelle principali sul DB runtime `1434`

| Tabella | Righe | Note |
|---|---|---|
| `Users_v2` | Da verificare read-only | Utenti WebApi |
| `Configurations` | Da verificare read-only | Parametri operativi |
| `MissionSessions` | Da verificare read-only | Report missioni |
| `MissionMarginSamples` | Da verificare read-only | Campioni margine missione |
| `Pc_CurrentStatus` | Da verificare read-only | Stato bot scritto dal Decisore e letto dalla WebApi |
| `Statistiche` | Da verificare read-only | Telemetry JSON letta dalla WebApi |
| `Margini` | Da verificare read-only | Serie margini Decisore |
| `UserPushSubscriptions` | Creata automaticamente se manca | Subscription Web Push degli utenti; schema idempotente lato WebApi, nessuna EF migration prod |

### Configurazioni chiave (da DB prod)

| Chiave | Valore | Significato |
|---|---|---|
| `STOP_WIN` | 500 | Target margine (€) |
| `STOP_LOSS` | -7000 | Stop loss massimo (€) |
| `STOP_TIME` | 760 | Minuti max per sessione |
| `RUNTIME_MODE` | Production | Modalità operativa |
| `DECISION_METHOD` | Engine2026 | Metodo decisionale |
| `BASE_UNIT` | 1 | Valore fiche unitaria |

---

## 4. FRONTEND

| Parametro | Valore |
|---|---|
| Hosting | Firebase — progetto **`eugenio-dashboard-2a`** |
| Site ID | `eugenio-dashboard-2a` |
| URL produzione | `https://eugenio-dashboard-2a.web.app/` — *validato HTTP 200* |
| Login UI | `https://eugenio-dashboard-2a.web.app/auth/login` |
| Root Owner (nascosta) | `https://eugenio-dashboard-2a.web.app/admin/root-owner` — solo `IsRootOwner=1`, non in menu |
| Account Firebase | `ak47129898@gmail.com` |
| API target (prod) | `https://vps-b0942869.vps.ovh.net` (`VITE_API_BASE_URL` in CI — **obbligatorio HTTPS** da Firebase) |
| API target (locale) | `http://localhost:5299` (`frontend/.env.example`) |
| Deploy ufficiale | GitHub Actions → `firebase-hosting-merge.yml` (root repo, `workflow_dispatch`, nome workflow: `Firebase Hosting Live`) |
| Config repo | `frontend/.firebaserc` → default `eugenio-dashboard-2a` |

> **Attenzione — dominio errato:** `https://eugenio-dashboard-2.web.app/` (senza `a` finale) **non** è DASH2A live: Firebase risponde **Site Not Found** (HTTP 404). Usare sempre **`eugenio-dashboard-2a.web.app`**.
>
> **Project ID:** `eugenio-dashboard-2a` (non `eugenio-dashboard-2`). Deploy CI root: `.github/workflows/firebase-hosting-merge.yml`.

---

## 5. BACKEND WEBAPI

| Parametro | Valore |
|---|---|
| Framework | .NET 9, ASP.NET Core |
| Porta locale | `5299` (HTTP) |
| Porta produzione | `80` + **`443` HTTPS** via IIS |
| URL produzione HTTP | `http://51.83.159.175` (legacy interno) |
| URL produzione HTTPS | **`https://vps-b0942869.vps.ovh.net`** (= `51.83.159.175`) |
| Autenticazione | JWT Bearer |
| Database ORM | Entity Framework Core |
| Realtime | SignalR (hub locale/prod — push VAPID opzionale) |

### Connection string produzione

Pattern (password in secret, **non** committare valori):

```text
Server=51.83.159.175,1434;Database=Eugenio-Demo10;User Id=sa3;Password=<SECRET>;Encrypt=False;TrustServerCertificate=True;
```

File runtime: override IIS `appsettings.Production.json` sul server. Non usare il valore di `backend/WebApi/appsettings.json` come fonte produzione.

### Endpoint principali

| Endpoint | Metodo | Descrizione |
|---|---|---|
| `/api/Auth/login` | POST | Login utente |
| `/api/Auth/test` | GET | Health check (smoke deploy) |
| `/api/runtime-mode` | GET/PUT | Production / Demo |
| `/api/Dashboard/data` | GET | Dati dashboard (**DB WebApi**) |
| `/api/mission/report/range` | GET | Report missioni per periodo |
| `/api/mission/reports/index` | GET | Indice sessioni |
| `/api/decider/config` | GET | Config Decider (diagnostica) |
| `/api/decider/health` | GET | Probe reachability Decider |
| `/api/push/vapid-public-key` | GET | Stato Web Push + public key VAPID; no auth, usato dal mobile admin |
| `/api/push/subscribe` | POST | Salva/aggiorna subscription Web Push dell'utente autenticato |
| `/api/admin/users/overview` | GET | Utenti e ruoli |
| `/api/admin/user-notification-settings` | GET/PUT | Notifiche email |
| `/api/admin/user-notification-settings/{id}/test` | POST | Test email |

### Push mobile admin / Web Push

| Parametro | Valore |
|---|---|
| Stato produzione | **Attivo** — `GET https://vps-b0942869.vps.ovh.net/api/push/vapid-public-key` → `enabled=true` |
| Config runtime | Sezione `Push` in `C:\inetpub\wwwroot\shared\appsettings.Production.json` e release corrente |
| Chiavi VAPID | Generate/applicate sul server dal workflow `configure-push-vapid.yml`; private key mai committata |
| Subject VAPID | Configurato nel runtime server dal workflow |
| Tabella DB | `UserPushSubscriptions`, creata idempotentemente dalla WebApi se assente |
| Invio notifiche | `MissionLifecycleService` invia push sugli eventi missione senza bloccare il flusso se il push fallisce |
| Frontend | `AdminMobileLive.vue` mostra differenza tra supporto browser, permesso utente, backend configurato e subscription attiva |

> Su iOS le notifiche Web Push richiedono app installata in Home Screen e permesso concesso. Su Android/Chrome basta browser compatibile + permesso notifiche.

---

## 6. CI/CD — GITHUB ACTIONS

| Parametro | Valore |
|---|---|
| Repo | `github.com/eugeniorossi2025-sudo/TradingDashboard-2a` |
| Clone locale corretto | `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri` |
| Remote atteso | `origin https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git` |
| Branch principale | `main` |
| Runner backend | **`dash2a-backend-runner-01`** |
| Runner backend labels | `self-hosted`, `Windows`, `X64`, `DASH2A`, `DASH2A-BACKEND` |
| Runner backend host | **`51.83.159.175`** (VPS backend, machine `WIN-P8JPV1DNSB6`) |
| Runner Decisore | **`dash2a-decisore-runner-01`** |
| Runner Decisore labels | `self-hosted`, `Windows`, `X64`, `DASH2A`, `DASH2A-DECISORE` |
| Runner Decisore host | **`51.178.16.37`** (VPS Decisore, machine `WIN-05FHTP223IE`) |
| Runner stato | **online** — *validato GitHub API 2026-05-27* |

### Workflows attivi

| File | Trigger | Azione |
|---|---|---|
| `deploy-backend-dash2a.yml` | `workflow_dispatch` | Build + deploy backend IIS (`demoapp`) |
| `enable-backend-https.yml` | `workflow_dispatch` | IIS 443 + cert Let's Encrypt |
| `firebase-hosting-merge.yml` | `workflow_dispatch` | Deploy frontend Firebase live |
| `firebase-hosting-pull-request.yml` | PR | Build frontend (no deploy live) |
| `deploy-decisore-safe.yml` | `workflow_dispatch` | Build + backup + deploy Decisore su `C:\Decisore` |
| `configure-push-vapid.yml` | `workflow_dispatch` | Genera/ruota/applica VAPID Web Push su WebApi prod e riavvia app pool `demoapp` |
| `diag-runtime-config-readonly.yml` | `workflow_dispatch` | Verifica read-only WebApi IIS + SQL `1434` |
| `diag-decisore-runner-readonly.yml` | `workflow_dispatch` | Verifica read-only runner Decisore, IIS, `C:\Decisore`, appsettings |

> **Nessun auto-deploy su push `main`** — backend, frontend e Decisore richiedono tutti `workflow_dispatch` manuale con input di conferma.
> **Decisore:** usare solo `DASH2A Decisore Deploy Safe`. Il vecchio `deploy-decisore-v2.yml` resta storico/obsoleto e non va ripristinato.
> **Push VAPID:** usare `configure-push-vapid.yml` solo quando manca la config o serve rotazione. Non inserire chiavi VAPID private nel repo, nei log o in `appsettings.json` committati.
> **Guardia repo:** i deploy DASH2A validi appaiono nei run di `eugeniorossi2025-sudo/TradingDashboard-2a` con workflow `DASH2A Backend Deploy Safe`, `DASH2A Decisore Deploy Safe` e `Firebase Hosting Live`. I run o remote di `TradingDashboard-iis` non sono prova del deploy DASH2A.

### Segreti GitHub (repository secrets — verificati 2026-05-25)

| Secret | Stato | Uso |
|---|---|---|
| `DASH2A_RDP_PASSWORD` | **OK** | RDP emergenza / readiness VPS |
| `FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2` | **OK** | Deploy Firebase (`eugenio-dashboard-2a`) |
| Connection string / JWT / SMTP | server-side | Override IIS — non in repo |
| `DECISORE_DB_PASSWORD` | **Non usare per deploy automatico** | Creato durante tentativo 2026-05-26; non agganciare a workflow deploy senza audit DB approvato |

Verifica secrets:

```powershell
gh secret list --repo eugeniorossi2025-sudo/TradingDashboard-2a
```

### Procedura deploy backend

```text
1. GitHub Actions → DASH2A Backend Deploy Safe → Run workflow
2. Input: I_UNDERSTAND_BACKEND_DEPLOY_ONLY
3. Smoke: http://51.83.159.175/api/Auth/test → 200
4. Login UI: https://eugenio-dashboard-2a.web.app/
```

### Procedura deploy frontend Firebase

```text
1. Verificare secret FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2 presente
2. GitHub Actions → Firebase Hosting Live → Run workflow (branch main)
3. Input: DEPLOY_FRONTEND
4. Smoke: https://eugenio-dashboard-2a.web.app/ → 200
```

Shell:

```powershell
gh workflow run "Firebase Hosting Live" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm_frontend_deploy=DEPLOY_FRONTEND
```

### Procedura configurazione Push VAPID

```text
1. Usare solo dopo deploy backend che contiene `/api/push/*`.
2. GitHub Actions → Configure Push VAPID → Run workflow.
3. Input standard: apply / no rotate; usare rotate solo se si vuole invalidare le vecchie subscription.
4. Il workflow aggiorna `C:\inetpub\wwwroot\shared\appsettings.Production.json` e la release corrente, poi riavvia app pool `demoapp`.
5. Smoke esterno: `https://vps-b0942869.vps.ovh.net/api/push/vapid-public-key` deve tornare `enabled=true`.
```

Shell:

```powershell
gh workflow run "Configure Push VAPID" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f mode=apply -f rotate=false
```

### Procedura deploy Decisore

```text
1. Verificare `dash2a-decisore-runner-01` online.
2. GitHub Actions → DASH2A Decisore Deploy Safe → Run workflow.
3. Input: DEPLOY_DECISORE.
4. Il workflow pubblica `decision-engine/Decisore/Decisore.csproj`, crea backup in `C:\DecisoreBackups`, preserva `C:\Decisore\appsettings.json`, riavvia app pool `Proactive`.
5. Smoke neutro: TCP `127.0.0.1:80=True`; `/api/proactive/health` può dare 404 ed è accettato perché la route non esiste.
```

> Per patch chirurgiche Decisore/frontend questa procedura rapida **non basta**: seguire sempre lo standard operativo obbligatorio sotto, con test locali, commit chirurgico, push controllato, approvazione deploy e blocco missione live.

Shell:

```powershell
gh workflow run "DASH2A Decisore Deploy Safe" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm=DEPLOY_DECISORE
```

## STANDARD OPERATIVO OBBLIGATORIO — PATCH CHIRURGICHE DASH2A

**Procedura ufficiale post-patch DASH2A.** Questo standard sostituisce qualunque flusso vecchio o incompleto per patch chirurgiche Decisore/frontend.

Validata sulla patch **Security Filter L6 Prevention** (`ProactiveEngine` + note UX frontend) e da riutilizzare per patch future analoghe.

Questa procedura vale per patch chirurgiche come correzioni in `decision-engine/Decisore/Engine/ProactiveEngine.cs` e sole note UX frontend in `StatsWidget.vue` / `ProfitsChats.vue`.

Sequenza obbligatoria:

1. Test locali.
2. Commit chirurgico.
3. Verifica diff.
4. Push controllato.
5. Verifica workflow.
6. Deploy manuali storici solo se approvati.
7. Stop obbligatorio prima di toccare missione live.
8. Nessuno smoke custom improvvisato.
9. Nessun workflow inventato.

**Regola assoluta:** se c'è una missione live attiva, **fermarsi prima di qualunque deploy, workflow, smoke remoto, chiamata HTTP al Decisore, restart app pool o diagnostica su runner**. Il workflow `DASH2A Decisore Deploy Safe` riavvia l'app pool `Proactive` e contiene uno step di smoke integrato; quindi si può lanciare solo dopo approvazione esplicita che conferma missione ferma o finestra operativa accettata.

#### Step 0 — Posizionamento e guardia repo

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
git remote -v
git status --short
git log --oneline -5
```

Esito richiesto:

- Remote `origin` = `https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git`.
- Branch previsto = `main` o branch patch approvato.
- Nessuna modifica estranea inclusa nella patch.
- Non usare `PCTEST45\TradingDashboard`, Dashboard 1 o repo `TradingDashboard-iis`.

#### Step 1 — Patch chirurgica

File ammessi per questa classe di intervento:

- `decision-engine/Decisore/Engine/ProactiveEngine.cs`
- `frontend/src/components/dashboard/StatsWidget.vue`
- `frontend/src/components/dashboard/ProfitsChats.vue`
- `tools/security-filter-l6-prevention-smoke.ps1`

Regole patch:

- Non fare refactor generale.
- Non toccare calcolo margini.
- Non toccare data logic del `Profits Chart`.
- Non toccare scaling `K`.
- Non toccare endpoint API.
- Non toccare logica L7/L8.
- Non modificare config live, DB, missione, runner o workflow per far passare il test.

Logica corretta `Security Filter`:

- Calcolare `securityFilterActive` prima del consumo credito L6.
- In caso `L5 loss + credito L6 disponibile + SecurityFilter score >= 3`, il filtro blocca prima di autorizzare L6.
- In quello scenario `ActionCode = 3` e `StopL6 = true`.
- `TotalSecurityFilterPreventedL6` aumenta solo quando una L6 reale sarebbe stata autorizzata ma viene bloccata.
- `TotalAuthL6Authorized` resta invariato.
- Il credito L6 non viene consumato.
- Nessun marker `LastL6Authorization...` deve essere scritto.

#### Step 2 — Test locali obbligatori, senza missione live

Eseguire solo test locali nel clone. Vietato chiamare endpoint live Decisore o WebApi per validare questa patch.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\security-filter-l6-prevention-smoke.ps1
dotnet build .\decision-engine\Decisore.sln
npm run build --prefix frontend
git diff --check
```

Esito richiesto dello smoke `security-filter-l6-prevention-smoke.ps1`:

- Scenario simulato: `L5 loss + credito L6 disponibile + SecurityFilter score >= 3`.
- `ActionCode = 3`.
- `StopL6 = true`.
- `TotalSecurityFilterPreventedL6 = 1`.
- `TotalAuthL6Authorized = 0`.
- Credito L6 non consumato.
- Nessun marker `LastL6Authorization...` scritto.

Esito richiesto build:

- `dotnet build .\decision-engine\Decisore.sln` = OK.
- `npm run build --prefix frontend` = OK.
- `git diff --check` = OK, nessun trailing whitespace.

#### Step 3 — Diff finale e commit locale

Prima del commit mostrare e verificare solo i file ammessi:

```powershell
git diff -- decision-engine/Decisore/Engine/ProactiveEngine.cs frontend/src/components/dashboard/StatsWidget.vue frontend/src/components/dashboard/ProfitsChats.vue tools/security-filter-l6-prevention-smoke.ps1
git status --short
```

Commit locale ammesso solo dopo conferma utente:

```powershell
git add decision-engine/Decisore/Engine/ProactiveEngine.cs frontend/src/components/dashboard/StatsWidget.vue frontend/src/components/dashboard/ProfitsChats.vue tools/security-filter-l6-prevention-smoke.ps1
git commit -m "Fix Security Filter L6 prevention semantics"
git rev-parse HEAD
```

Dopo il commit ripetere i test locali dello Step 2. Non fare push senza conferma esplicita.

#### Step 4 — Push controllato

Eseguire solo se l'utente approva esplicitamente il push:

```powershell
git push origin main
git rev-parse HEAD
gh run list --repo eugeniorossi2025-sudo/TradingDashboard-2a --limit 8
```

Esito richiesto:

- Hash locale = hash pushato.
- Nessun deploy automatico inatteso.
- Nessun workflow manuale lanciato.
- Missione live invariata.

#### Step 5 — Valutazione deploy, senza automatismi

Prima di qualunque deploy chiedere conferma separata per ogni componente:

- Decisore: serve solo se cambia `ProactiveEngine.cs`.
- Firebase: serve solo se cambiano note UX frontend.
- Backend WebApi: non serve per questa patch.

**Blocco sicurezza missione live:** prima del deploy Decisore l'utente deve confermare una delle due condizioni:

- `MISSIONE_FERMA_CONFERMATA`
- `AUTORIZZO_DEPLOY_DECISORE_CON_RESTART_E_SMOKE_INTEGRATO`

Senza una di queste conferme, **non lanciare**:

- `DASH2A Decisore Deploy Safe`
- `DIAG - Decisore runner readonly`
- chiamate a `/api/proactive/*`
- `Test-NetConnection` remoto
- qualunque smoke remoto/custom
- restart app pool `Proactive`

#### Step 6 — Deploy manuale approvato

Se e solo se approvato, usare i workflow storici esistenti. Non inventare workflow, script, runner label o procedure.

Decisore:

```powershell
gh workflow run "DASH2A Decisore Deploy Safe" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm=DEPLOY_DECISORE
```

Firebase:

```powershell
gh workflow run "Firebase Hosting Live" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm_frontend_deploy=DEPLOY_FRONTEND
```

Monitoraggio consentito solo del run GitHub appena lanciato:

```powershell
gh run watch <RUN_ID> --repo eugeniorossi2025-sudo/TradingDashboard-2a --exit-status
gh run view <RUN_ID> --repo eugeniorossi2025-sudo/TradingDashboard-2a --json databaseId,name,headSha,status,conclusion,url
```

Non aggiungere smoke esterni dopo il deploy, salvo nuova conferma esplicita dell'utente.

#### Step 7 — Output finale obbligatorio

Il report finale deve contenere:

- Commit hash locale e remoto.
- File modificati.
- Test locali eseguiti e risultato.
- Push OK/KO.
- Workflow eventualmente lanciati, run ID, commit `headSha`, stato finale.
- Conferma che non sono stati toccati missione live, DB, endpoint API, calcolo margini, scaling `K`, logica L7/L8.
- Se il Decisore è stato deployato: dichiarare esplicitamente che il workflow ha riavviato `Proactive` e ha eseguito lo smoke integrato previsto dal workflow.

### Diagnostiche read-only post-deploy

```powershell
# WebApi runtime + SQL 1434
gh workflow run "DASH2A Runtime Config Readonly Diagnostic" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm=RUNTIME_CONFIG_READONLY

# Decisore IIS + C:\Decisore + appsettings
gh workflow run "DIAG - Decisore runner readonly" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm=DECISORE_READONLY
```

---

## 7. CREDENZIALI RIEPILOGO (senza valori)

| Servizio | Utente | Dove trovare la password |
|---|---|---|
| OVH VPS Backend RDP | `administrator` | GitHub Secret `DASH2A_RDP_PASSWORD` |
| OVH VPS Decisore RDP | `administrator` | Secret / vault operativo |
| SQL Server runtime WebApi + Decisore | `sa3` | `C:\inetpub\wwwroot\shared\appsettings.Production.json` e `C:\Decisore\appsettings.json` live; non documentare password |
| Dashboard Web App admin | `admin` | DB `Users_v2` prod / seed locale |
| Gmail SMTP | `eugeniorosii2025@gmail.com` | App password in secret server |
| Firebase | `ak47129898@gmail.com` | Console Firebase `eugenio-dashboard-2a` |
| OVH Account | `eugeniobac2@outlook.it` | Pannello OVH |

---

## 8. PRODUZIONE — RIEPILOGO URL

| Componente | URL / host |
|---|---|
| Frontend | `https://eugenio-dashboard-2a.web.app` |
| WebApi HTTPS | **`https://vps-b0942869.vps.ovh.net`** |
| WebApi IP | `51.83.159.175` |
| Push VAPID status | `https://vps-b0942869.vps.ovh.net/api/push/vapid-public-key` |
| DB runtime WebApi + Decisore | `51.83.159.175:1434` / `Eugenio-Demo10` |
| Decisore (engine) | `http://51.178.16.37` / `/api/proactive` |
| Fonte dati dashboard Vue | **UI → WebApi HTTPS → DB runtime 1434** |

---

## 9. AMBIENTE LOCALE (LocalProdLike)

### Flusso dati

```text
Frontend locale     http://localhost:5001
       ↓  VITE_API_BASE_URL=http://localhost:5299
WebApi locale       http://localhost:5299
       ↓  Dash2A_LocalProdLike @ (localdb)\MSSQLLocalDB
DB locale           auth, missioni, Pc_CurrentStatus, Configurations merge

WebApi locale  ──(solo diagnostica)──►  http://51.178.16.37/api/proactive
                                        /api/decider/config
                                        /api/decider/health
```

**Non implementato:** sync Decisore → DB locale; dashboard live dal Decisore.

| Parametro | Valore |
|---|---|
| Frontend | `http://localhost:5001` |
| WebApi | `http://localhost:5299` |
| DB | `(localdb)\MSSQLLocalDB` / `Dash2A_LocalProdLike` |
| Profilo ASP.NET | `LocalProdLike` → `appsettings.LocalProdLike.json` |
| Decider (health/config) | `http://51.178.16.37/api/proactive` |
| Admin locale | `admin` / password seed locale (non prod) |
| Config riferimento | `ops/dash2a-readiness/local-prod-like.env.example` |

### Avvio locale

```powershell
.\restart-app-safe.ps1 -Run
```

### Merge configurazioni mancanti (read-only server)

```powershell
powershell -ExecutionPolicy Bypass -File .\ops\dash2a-readiness\merge-missing-from-prod-readonly.ps1
```

- SELECT remoto da `appsettings.json`; INSERT solo su locale
- Backup in `ops/dash2a-readiness/backups/` (gitignored)
- Chiavi priority: `DECISION_METHOD`, `STOP_WIN`, `STOP_TIME`, `STOP_LOSS`, `RUNTIME_MODE`, `BASE_UNIT`

---

## 10. REGOLE OPERATIVE

1. **NON** eseguire `dotnet ef database update` in produzione.
2. **NON** modificare `Users_v2`, `Configurations`, `AspNetRoles` senza review.
3. **NON** deployare frontend Firebase senza backend live.
4. **NON** usare credenziali Dashboard 1 in DASH2A.
5. Backup prima di modifiche DB produzione.
6. Deploy backend solo via workflow manuale con conferma.
7. Smoke `/api/Auth/test` → 200 obbligatorio post-deploy.
8. **NON** usare `51.210.181.37` per Decider — obsoleto.
9. Deploy Decisore solo via workflow `DASH2A Decisore Deploy Safe`, con backup automatico e `appsettings.json` live preservato.
10. **NON** applicare migration/script SQL sul DB Decisore senza audit read-only e backup/diff delle stored procedure esistenti.
11. **NON** usare `/api/proactive/reset` come healthcheck neutro: ha side effect.
12. **NON** committare chiavi VAPID private; la configurazione push vive nel runtime server e si gestisce con `configure-push-vapid.yml`.
13. **NON** patchare GameBot fuori da `tools/eugenio-bot` né deployare exe senza commit/tag su `TradingDashboard-2a` (vedi §14).

---

## 11. DECISORE — STATO OPERATIVO (2026-05-27)

### Stato produzione verificato

| Punto | Stato |
|---|---|
| Runtime attivo | `C:\Decisore` |
| IIS site | `default` |
| App pool | `Proactive` (Started) |
| DB configurato | **`51.83.159.175,1434`** / `Eugenio-Demo10` / login `sa3` |
| Deploy CI/CD | **Attivo** — `DASH2A Decisore Deploy Safe` su runner `dash2a-decisore-runner-01` |
| Backup deploy | `C:\DecisoreBackups\decisore-YYYYMMDD-HHmmss` |
| Binario live verificato | `C:\Decisore\Decisore.dll` aggiornato al deploy `2026-05-27 02:47` |
| HTTP status neutro | TCP 80 OK; `/api/proactive/health` = 404 atteso perché route non esiste |
| Ultimo deploy | 2026-05-27 — patch Security Filter per-bot |

### Cronologia fix 2026-05-26

| Problema | Causa | Fix applicato |
|---|---|---|
| `HTTP 500.30` startup crash | `appsettings.json` Decisore puntava a un DB errato | Modificato manualmente a `51.83.159.175,1434` |
| Drift storico `1433` | Vecchi script/config avevano creato o letto oggetti sull'istanza sbagliata | Runtime consolidato su `1434`; trattare `1433` solo come storico/obsoleto |
| `Invalid object name 'dbo.Pc_CurrentStatus_PBT_History'` | Tabella mancante | Creata su `51.83.159.175,1434` |
| TCP timeout 10060 su `1434` | Firewall Windows bloccava porta 1434 da IP Decisore | Aggiunta regola `SQLEXPRESS01-1434-Decisore` (inbound TCP 1434, remote `51.178.16.37`) |

### DB Decisore — oggetti verificati su `51.83.159.175,1434`

**Tabelle (tutte OK):**

| Tabella | Stato | Note |
|---|---|---|
| `dbo.Pc_CurrentStatus` | **OK** | Preesistente |
| `dbo.Pc_CurrentStatus_PBT_History` | **OK** | Creata 2026-05-26 |
| `dbo.Margini` | **OK** | Preesistente |
| `dbo.Statistiche` | **OK** | Creata 2026-05-26 |
| `dbo.ApiConfigurations` | **OK** | Creata 2026-05-26 |
| `dbo.ApiLogs` | **OK** | Creata 2026-05-26 |
| `dbo.Configurations` | **OK** | Preesistente |

**Stored Procedure:**

| SP | Stato | Note |
|---|---|---|
| `UpS_Users_Api` | **OK** | Validazione utente |
| `Upsert_Pc_CurrentStatus` | **OK** | Update full |
| `Upsert_Pc_CurrentStatus_Simple` | **OK** | Update parziale |
| `Upsert_Pc_CurrentStatus_Deck` | **OK** | Update mazzo |
| `AggiornaStatistiche` | **OK** | Aggiorna statistiche |
| `InsertMargine` | **OK** | Inserisce margine |
| `upI_Values` | **MANCANTE** | Non critica per startup; usata solo in `SaveRequestValue` (fire & forget) |

### Regola firewall attiva (VPS backend `51.83.159.175`)

```text
Nome: SQLEXPRESS01-1434-Decisore
Direzione: Inbound
Protocollo: TCP
Porta locale: 1434
IP remoto ammesso: 51.178.16.37
Azione: Allow
Stato: Enabled
```

**Non rimuovere questa regola** — senza di essa il Decisore non si avvia.

### Riavvio manuale app pool (dal VNC/RDP del VPS Decisore)

```powershell
C:\Windows\System32\inetsrv\appcmd stop apppool /apppool.name:"Proactive"
C:\Windows\System32\inetsrv\appcmd start apppool /apppool.name:"Proactive"
Start-Sleep 6
Test-NetConnection 127.0.0.1 -Port 80
```

---

## 12. STATO VALIDATO (2026-05-27)

| # | Controllo | Esito | Note |
|---|---|---|---|
| 1 | Runner `dash2a-backend-runner-01` online | **OK** | labels: self-hosted, Windows, X64, DASH2A, DASH2A-BACKEND |
| 2 | Runner su VPS backend `51.83.159.175` | **OK** | self-hosted deploy workflow |
| 2b | Runner `dash2a-decisore-runner-01` online | **OK** | labels: self-hosted, Windows, X64, DASH2A, DASH2A-DECISORE |
| 3 | WebApi prod `/api/Auth/test` | **OK** | HTTP 200, IIS 10 |
| 4 | SQL prod runtime porta `1434` | **OK** | WebApi live e Decisore puntano a `51.83.159.175,1434` |
| 5 | SQL login `sa3` / `Eugenio-Demo10` | **OK** | read-only diagnostic su runtime |
| 6 | Frontend prod Firebase | **OK** | HTTP 200, build da GitHub Actions |
| 7 | Decider `51.178.16.37` | **OK** | TCP 80 OK; `C:\Decisore`, IIS `default`, app pool `Proactive` verificati |
| 8 | Decider obsoleto `51.210.181.37` | **404** | non usare |
| 9 | Stack locale `:5299` / `:5001` | **OK** | smoke sessione corrente |
| 10 | Decider locale health/config | **OK** | diagnostica only, no sync DB |
| 11 | WebApi prod dati dashboard | **OK** | da DB runtime unico `51.83.159.175,1434` |
| 12 | Secret `FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2` | **OK** | configurato su GitHub 2026-05-25 |
| 13 | Firebase project ID workflow | **OK** | `eugenio-dashboard-2a` (commit `bd6c322`) |
| 14 | Push VAPID prod `/api/push/vapid-public-key` | **OK** | HTTP 200, `enabled=true`, public key presente |

### Punti ancora incerti / da monitorare

| Punto | Stato |
|---|---|
| SP `upI_Values` mancante su `1434` | **Da creare** — non critica per startup; blocca solo `SaveRequestValue` (fire & forget) |
| SignalR / push VAPID in locale | Opzionale — `enabled=false` atteso se VAPID non configurato |
| `MissionSessions` prod vuota vs locale ricca | Locale ha dati demo/import; prod 0 al check — non allineare automaticamente |
| Allineamento binario live `C:\Decisore` vs codice repo | **OK** dopo deploy `8e2efe1` (2026-06-05); verificare di nuovo prima di ogni release |

---

## 13. DEPLOY PRODUZIONE — STATO TOTALE (2026-06-05)

### Riferimenti repo (produzione)

| Voce | Valore |
|---|---|
| Repo GitHub | `eugeniorossi2025-sudo/TradingDashboard-2a` |
| Clone locale | `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri` |
| Branch produzione | `main` |
| Commit deployato (tip) | **`8e2efe1`** — `feat(spot): default OFF, module absent when disabled, fail-safe config` |
| Commit inclusi | **`63fba5f`** e tutti gli antenati fino a `8e2efe1` (Control Room, Spot L6, Root Owner, auth fix, collaudo, ecc.) |
| Data deploy stack completo | **2026-06-05** (~15:03 UTC) |

### Ultimo deploy stack completo (2026-06-05)

| Componente | Stato | Branch / run | Dettaglio |
|---|---|---|---|
| **Backend WebApi IIS** | **DEPLOY OK** | `main` run [`27022669207`](https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a/actions/runs/27022669207) | Workflow `DASH2A Backend Deploy Safe`; headSha `8e2efe1` |
| **Frontend Firebase** | **DEPLOY OK** | run [`27022671996`](https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a/actions/runs/27022671996) | Workflow `Firebase Hosting Live`; headSha `8e2efe1`; `VITE_API_BASE_URL=https://vps-b0942869.vps.ovh.net` |
| **Decisore live** | **DEPLOY OK** | run [`27022666669`](https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a/actions/runs/27022666669) | Workflow `DASH2A Decisore Deploy Safe`; headSha `8e2efe1`; IIS `default` + app pool `Proactive` → `C:\Decisore`; DB `51.83.159.175,1434` |
| **Backend HTTPS IIS** | **OK** | run `26404743108` | `enable-backend-https.yml` — cert LE su hostname OVH |
| **Login UI + /pages/user** | **OK** | smoke 2026-05-25 | Mixed Content risolto (HTTPS end-to-end) |
| **Frontend live URL** | **OK** | — | `https://eugenio-dashboard-2a.web.app` |
| **Backend live URL HTTPS** | **OK** | — | `https://vps-b0942869.vps.ovh.net` |
| **Stack prod allineato** | **OK** | — | Frontend HTTPS → WebApi HTTPS → DB `51.83.159.175,1434` |
| **Push mobile admin** | **ATTIVO** | workflow `configure-push-vapid.yml` OK | `/api/push/vapid-public-key` → `enabled=true`; VAPID in runtime server, private key non in repo |

### Deploy precedente (2026-05-27 — storico)

| Componente | Run | Dettaglio |
|---|---|---|
| Backend WebApi IIS | `26483836820` | release `backend-20260527-024817` |
| Frontend Firebase | `26483836854` | — |
| Decisore live | `26483836856` | `Decisore.dll` 2026-05-27 02:47 |

### Run fallite (storico sessione — risolte)

| Run | Workflow | Causa | Risoluzione |
|---|---|---|---|
| `26400373057` | Firebase Hosting Live | Secret Firebase assente + project ID errato | Secret creato + fix `bd6c322` → run `26402801288` OK |
| `26483556956` | DASH2A Decisore Deploy Safe | `robocopy` exit code 1 trattato come failure anche se backup OK | Fix workflow `36e4947`, run successivi OK |

### Comandi verifica rapida post-deploy

```powershell
# Backend
Invoke-WebRequest -Uri "http://51.83.159.175/api/Auth/test" -UseBasicParsing

# Frontend
Invoke-WebRequest -Uri "https://eugenio-dashboard-2a.web.app/" -UseBasicParsing

# Secrets
gh secret list --repo eugeniorossi2025-sudo/TradingDashboard-2a

# Ultimi deploy Actions
gh run list --repo eugeniorossi2025-sudo/TradingDashboard-2a --limit 5

# Decisore read-only inventory
gh workflow run "DIAG - Decisore runner readonly" --repo eugeniorossi2025-sudo/TradingDashboard-2a -f confirm=DECISORE_READONLY
```

---


## 15. CHECKLIST INIZIO SESSIONE

- [ ] Runner `dash2a-backend-runner-01` online (GitHub → Actions → Runners)
- [ ] Runner `dash2a-decisore-runner-01` online se si lavora sul Decisore
- [ ] `http://51.83.159.175/api/Auth/test` → 200
- [ ] `https://eugenio-dashboard-2a.web.app/` carica
- [ ] `git branch` = `main`, `git status` pulito
- [ ] `git log --oneline -5`
- [ ] Decider prod = `51.178.16.37` (non `51.210.181.37`)
- [ ] Decisore live path = `C:\Decisore`, app pool = `Proactive`, backup root = `C:\DecisoreBackups`
- [ ] GameBot: sorgente = `tools/eugenio-bot` §14; runner attivo = `relisegiacomo ok 1.6`; hash vs tabella §14.3
---

## Checklist agente giacom (obbligatoria)

- [ ] `git rev-parse --show-toplevel` = `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`
- [ ] `git remote -v` contiene `TradingDashboard-2a` (non `TradingDashboard-iis`)
- [ ] Branch = `main` (salvo branch patch approvato)
- [ ] **Non** letto §14 / `tools/eugenio-bot` / `AGENT-GIACOMO1.md`
- [ ] **Non** letto doc Eugenio / `marco` / `marco1`
- [ ] Smoke post-deploy WebApi: `GET https://vps-b0942869.vps.ovh.net/api/Auth/test` → 200
- [ ] Frontend prod: `https://eugenio-dashboard-2a.web.app/` (non `eugenio-dashboard-2.web.app`)
- [ ] **Fine scope:** sezione **`Stato sessione`** aggiornata in **questo file**
