# DASH2A — Infrastruttura Definitiva

> **Leggere questo file all'inizio di ogni sessione di lavoro.**
> Aggiornare quando cambiano IP, credenziali, o configurazioni.
> **Ultimo aggiornamento: 2026-05-26 (sera)** — Decisore completamente operativo: DB corretto su `51.83.159.175,1434`, tabelle create, firewall aperto. Vedi §11.

---

## 1. SISTEMI COINVOLTI — NON MESCOLARE

| Sistema | Repo | Scopo |
|---|---|---|
| **Dashboard 1** | `PCTEST45\TradingDashboard` | Sistema legacy — bot / Gamebot storico |
| **DASH2A** | `NuovaDashboard-MarcoTurri` | Nuovo sistema — questo repo |

**Regola assoluta:** credenziali, IP Firebase, e DB di Dashboard 1 non si usano per deploy DASH2A e viceversa.

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
| Porte verificate | **80** HTTP (WebApi OK), **1433** SQL (TcpTest OK), **3389** RDP |
| HTTPS | Porta **443 attiva** — Let's Encrypt su `vps-b0942869.vps.ovh.net` |
| Smoke test HTTP | `GET http://51.83.159.175/api/Auth/test` → **200** |
| Smoke test HTTPS | `GET https://vps-b0942869.vps.ovh.net/api/Auth/test` → **200** |

**Architettura dati produzione:** la WebApi dashboard legge e scrive **solo** sul DB SQL locale al VPS backend (`51.83.159.175` / `Eugenio-Demo10`). **Non** legge il DB del Decisore. Non esiste sync automatico Decisore → DB WebApi nel repo.

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
| Deploy Decisore | **Manuale / non automatizzato** |
| Stato 2026-05-26 | **OPERATIVO** — HTTP 200 su `/api/proactive/reset` e `/api/proactive/decide` |

> **ATTENZIONE:** il Decisore usa il DB sul **VPS backend** (`51.83.159.175,1434`), NON un DB locale a se stesso. La porta 1434 è SQLEXPRESS01, istanza separata da quella usata dalla WebApi (1433).

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

> **Non esiste `/health`** — usare `/reset` solo per test manuali (ha side effect: svuota tabelle).

**Locale:** la WebApi in `LocalProdLike` espone `/api/decider/config` e `/api/decider/health` come **sonda diagnostica** verso `51.178.16.37`. **Non** sincronizza `Pc_CurrentStatus` né alimenta la dashboard locale.

### 2.3 VPS obsoleta (solo riferimento storico)

| Parametro | Valore |
|---|---|
| Hostname | `vps-138a2a47.vps.ovh.net` |
| IPv4 | ~~`51.210.181.37`~~ |
| Stato | **Non usare** — probe `/api/proactive/reset` → 404 (2026-05-25) |
| Note | Sostituita da `51.178.16.37` per API proactive; rimuovere da config nuove |

---

## 3. DATABASE PRODUZIONE (WebApi dashboard)

| Parametro | Valore |
|---|---|
| Host | `51.83.159.175,1433` |
| Database | `Eugenio-Demo10` |
| Login | `sa3` |
| Password | **Secret / local config** (`appsettings.json`, GitHub Secrets) — non documentare in chiaro |
| Encrypt | False |
| Login verificato | **OK** (2026-05-25) |
| Porta SQL verificata | **1433 OK** |
| EF Migrations | **NON eseguire** — `__EFMigrationsHistory` vuota per design |

### Tabelle principali (conteggi verificati 2026-05-25, read-only)

| Tabella | Righe | Note |
|---|---|---|
| `Users_v2` | 5 | admin, Giacomo, test, Marko, marcoadmin |
| `Configurations` | 20 | Parametri operativi |
| `MissionSessions` | 0 | Vuota al momento del check |
| `MissionMarginSamples` | 0 | Vuota al momento del check |
| `Pc_CurrentStatus` | 0 | Vuota al momento del check |
| `UserNotificationSettings` | 0 | Da popolare se servono notifiche prod |

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
| Account Firebase | `ak47129898@gmail.com` |
| API target (prod) | `https://vps-b0942869.vps.ovh.net` (`VITE_API_BASE_URL` in CI — **obbligatorio HTTPS** da Firebase) |
| API target (locale) | `http://localhost:5299` (`frontend/.env.example`) |
| Deploy ufficiale | GitHub Actions → `firebase-hosting-merge.yml` (`workflow_dispatch`) |
| Config repo | `frontend/.firebaserc` → default `eugenio-dashboard-2a` |

> **Attenzione:** il project ID Firebase corretto è **`eugenio-dashboard-2a`**, non `eugenio-dashboard-2` (vecchio riferimento errato nel repo, corretto commit `bd6c322`).

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
Server=51.83.159.175,1433;Database=Eugenio-Demo10;User Id=sa3;Password=<SECRET>;Encrypt=False;TrustServerCertificate=True;
```

File: `backend/WebApi/appsettings.json` + override IIS `appsettings.Production.json` sul server.

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
| `/api/admin/users/overview` | GET | Utenti e ruoli |
| `/api/admin/user-notification-settings` | GET/PUT | Notifiche email |
| `/api/admin/user-notification-settings/{id}/test` | POST | Test email |

---

## 6. CI/CD — GITHUB ACTIONS

| Parametro | Valore |
|---|---|
| Repo | `github.com/eugeniorossi2025-sudo/TradingDashboard-2a` |
| Branch principale | `main` |
| Runner self-hosted | **`dash2a-backend-runner-01`** |
| Runner labels | `self-hosted`, `Windows`, `X64`, `DASH2A`, `DASH2A-BACKEND` |
| Runner host | **`51.83.159.175`** (VPS backend) |
| Runner stato | **online** — *validato GitHub API 2026-05-25* |

### Workflows attivi

| File | Trigger | Azione |
|---|---|---|
| `deploy-backend-dash2a.yml` | `workflow_dispatch` | Build + deploy backend IIS |
| `enable-backend-https.yml` | `workflow_dispatch` | IIS 443 + cert Let's Encrypt |
| `firebase-hosting-merge.yml` | `workflow_dispatch` | Deploy frontend Firebase live |
| `firebase-hosting-pull-request.yml` | PR | Build frontend (no deploy live) |

> **Nessun auto-deploy su push `main`** — backend e frontend richiedono entrambi `workflow_dispatch` manuale.
> **Decisore:** nessun workflow deploy attivo nel repo dopo il rollback infra `b104502`. Il workflow sperimentale `deploy-decisore-v2.yml` è stato rimosso dal repo e disabilitato lato GitHub durante la stabilizzazione.

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

---

## 7. CREDENZIALI RIEPILOGO (senza valori)

| Servizio | Utente | Dove trovare la password |
|---|---|---|
| OVH VPS Backend RDP | `administrator` | GitHub Secret `DASH2A_RDP_PASSWORD` |
| OVH VPS Decisore RDP | `administrator` | Secret / vault operativo |
| SQL Server WebApi | `sa3` | `appsettings.json` locale, secret server IIS |
| SQL Server Decisore | `sa` (o equivalente) | `decision-engine/Decisore/appsettings.json`, secret VPS Decisore |
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
| DB dashboard | `51.83.159.175:1433` / `Eugenio-Demo10` |
| Decisore (engine) | `http://51.178.16.37` / `/api/proactive` |
| Fonte dati dashboard Vue | **WebApi → DB backend** (non Decisore) |

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
9. **NON** deployare il Decisore via GitHub Actions finché non esiste un piano approvato.
10. **NON** applicare migration/script SQL sul DB Decisore senza audit read-only e backup/diff delle stored procedure esistenti.
11. **NON** usare `/api/proactive/reset` come healthcheck neutro: ha side effect.

---

## 11. DECISORE — STATO OPERATIVO (2026-05-26)

### Stato produzione verificato

| Punto | Stato |
|---|---|
| Runtime attivo | `C:\Decisore` |
| IIS site | `default` |
| App pool | `Proactive` (Started) |
| DB configurato | **`51.83.159.175,1434`** / `Eugenio-Demo10` / login `sa3` |
| Deploy CI/CD | **Non attivo** — solo manuale |
| HTTP status | **200 OK** — `/api/proactive/reset` risponde da interno e da internet |
| Ultimo fix | 2026-05-26 sera — vedere log sotto |

### Cronologia fix 2026-05-26

| Problema | Causa | Fix applicato |
|---|---|---|
| `HTTP 500.30` startup crash | `appsettings.json` puntava a `51.178.16.37,1433` (locale VPS) | Modificato manualmente a `51.83.159.175,1434` |
| `Invalid object name 'dbo.Statistiche'` | Tabella mancante su `1433` | Creata su `1433` (obsoleto) poi su `1434` (corretto) |
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

### Riavvio manuale app pool (dal VNC del VPS Decisore)

```powershell
C:\Windows\System32\inetsrv\appcmd stop apppool /apppool.name:"Proactive"
C:\Windows\System32\inetsrv\appcmd start apppool /apppool.name:"Proactive"
Start-Sleep 6
Invoke-WebRequest http://127.0.0.1/api/proactive/reset -UseBasicParsing
```

---

## 12. STATO VALIDATO (2026-05-26)

| # | Controllo | Esito | Note |
|---|---|---|---|
| 1 | Runner `dash2a-backend-runner-01` online | **OK** | labels: self-hosted, Windows, X64, DASH2A, DASH2A-BACKEND |
| 2 | Runner su VPS backend `51.83.159.175` | **OK** | self-hosted deploy workflow |
| 3 | WebApi prod `/api/Auth/test` | **OK** | HTTP 200, IIS 10 |
| 4 | SQL prod porta 1433 | **OK** | TcpTestSucceeded |
| 5 | SQL login `sa3` / `Eugenio-Demo10` | **OK** | read-only query |
| 6 | Frontend prod Firebase | **OK** | HTTP 200, build da GitHub Actions |
| 7 | Decider `51.178.16.37/api/proactive/reset` | **OK** | HTTP 200 (verificato 2026-05-26 sera) — side effect: non usare come healthcheck neutro |
| 8 | Decider obsoleto `51.210.181.37` | **404** | non usare |
| 9 | Stack locale `:5299` / `:5001` | **OK** | smoke sessione corrente |
| 10 | Decider locale health/config | **OK** | diagnostica only, no sync DB |
| 11 | WebApi prod dati dashboard | **OK** | da DB `51.83.159.175`, non Decisore |
| 12 | Secret `FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2` | **OK** | configurato su GitHub 2026-05-25 |
| 13 | Firebase project ID workflow | **OK** | `eugenio-dashboard-2a` (commit `bd6c322`) |

### Punti ancora incerti / da monitorare

| Punto | Stato |
|---|---|
| SP `upI_Values` mancante su `1434` | **Da creare** — non critica per startup; blocca solo `SaveRequestValue` (fire & forget) |
| SignalR / push VAPID in locale | Opzionale — 404 atteso se non configurato |
| `MissionSessions` prod vuota vs locale ricca | Locale ha dati demo/import; prod 0 al check — non allineare automaticamente |
| Allineamento binario live `C:\Decisore` vs codice repo | Da verificare prima di qualsiasi nuovo deploy |

---

## 13. DEPLOY PRODUZIONE — STATO TOTALE (2026-05-26)

| Componente | Stato | Branch / run | Dettaglio |
|---|---|---|---|
| **Backend WebApi IIS** | **DEPLOY OK** | `main` run `26400202381` | Workflow `DASH2A Backend Deploy Safe` |
| **Backend HTTPS IIS** | **OK** | run `26404743108` | `enable-backend-https.yml` — cert LE su hostname OVH |
| **Frontend Firebase** | **DEPLOY OK** | run `26404807343` | `VITE_API_BASE_URL=https://vps-b0942869.vps.ovh.net` |
| **Login UI + /pages/user** | **OK** | smoke 2026-05-25 | Mixed Content risolto (HTTPS end-to-end) |
| **Frontend live URL** | **OK** | — | `https://eugenio-dashboard-2a.web.app` |
| **Backend live URL HTTPS** | **OK** | — | `https://vps-b0942869.vps.ovh.net` |
| **Stack prod allineato** | **OK** | — | Frontend HTTPS → WebApi HTTPS → DB `51.83.159.175` |
| **Decisore live** | **OPERATIVO** | fix 2026-05-26 sera | IIS `default` + app pool `Proactive` → `C:\Decisore`; DB `51.83.159.175,1434`; HTTP 200 |

### Run fallite (storico sessione — risolte)

| Run | Workflow | Causa | Risoluzione |
|---|---|---|---|
| `26400373057` | Firebase Hosting Live | Secret Firebase assente + project ID errato | Secret creato + fix `bd6c322` → run `26402801288` OK |

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
```

---

## 14. CHECKLIST INIZIO SESSIONE

- [ ] Runner `dash2a-backend-runner-01` online (GitHub → Actions → Runners)
- [ ] `http://51.83.159.175/api/Auth/test` → 200
- [ ] `https://eugenio-dashboard-2a.web.app/` carica
- [ ] `git branch` = `main`, `git status` pulito
- [ ] `git log --oneline -5`
- [ ] Decider prod = `51.178.16.37` (non `51.210.181.37`)
