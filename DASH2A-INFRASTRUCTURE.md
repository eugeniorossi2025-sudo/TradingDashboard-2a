# DASH2A — Infrastruttura Definitiva

> **Leggere questo file all'inizio di ogni sessione di lavoro.**
> Aggiornare quando cambiano IP, credenziali, o configurazioni.
> Ultimo aggiornamento: 2026-05-25

---

## 1. SISTEMI COINVOLTI — NON MESCOLARE

| Sistema | Repo | Scopo |
|---|---|---|
| **Dashboard 1** | `PCTEST45\TradingDashboard` | Sistema legacy — bot BOTITALIA → `51.178.16.37` |
| **DASH2A** | `NuovaDashboard-MarcoTurri` | Nuovo sistema — questo repo |

**Regola assoluta:** credenziali, IP, Firebase, e DB di Dashboard 1 non si usano mai in DASH2A e viceversa.

---

## 2. INFRASTRUTTURA SERVER DASH2A

### 2.1 VPS Backend (OVH)
| Parametro | Valore |
|---|---|
| Provider | OVH |
| Hostname | `vps-b0942869.vps.ovh.net` |
| IPv4 | `51.83.159.175` |
| OS | Windows Server 2025 Standard (Desktop) |
| Piano | VPS-2, 6 vCore, 12 GB RAM, 100 GB |
| Web Server | IIS 10 |
| App Pool | `demoapp` |
| Publish path | `C:\inetpub\wwwroot\publish` |
| Release root | `C:\inetpub\wwwroot\releases` |
| Backup root | `C:\inetpub\wwwroot\backups` |
| RDP user | `administrator` |
| RDP password | In GitHub Secret `DASH2A_RDP_PASSWORD` |
| Porte aperte | 80 (HTTP), 1433 (SQL), 3389 (RDP) |
| HTTPS | Port 443 non attiva — solo HTTP |

### 2.2 VPS Decisore (OVH — separato)
| Parametro | Valore |
|---|---|
| Hostname | `vps-138a2a47.vps.ovh.net` |
| IPv4 | `51.210.181.37` |
| Ruolo | Decision Engine autonomo |
| DB | `Eugenio-Demo10` (istanza separata) |
| SQL login | `sa` / `LionGest123@` |
| Repo path | `decision-engine/Decisore/` |

> Il Decisore è **completamente separato** dal WebApi backend. Non condividono DB.

---

## 3. DATABASE PRODUZIONE (WebApi)

| Parametro | Valore |
|---|---|
| Host | `51.83.159.175,1433` |
| Database | `Eugenio-Demo10` |
| Login | `sa3` |
| Password | `LionGest123@` |
| Encrypt | False |
| EF Migrations | **NON eseguire** — `__EFMigrationsHistory` è vuota |

### Tabelle principali
| Tabella | Righe | Note |
|---|---|---|
| `Users_v2` | 5 | admin, Giacomo, test, Marko, marcoadmin |
| `AspNetRoles` | 3 | Admin, User, BotOperator |
| `Configurations` | 20 | Parametri operativi — NON toccare senza review |
| `MissionSessions` | 0 | Si riempie live |
| `MissionMarginSamples` | 0 | Si riempie live |
| `UserNotificationSettings` | 0 | Aggiungere manualmente dopo deploy |
| `UserAccessEvents` | 0 | Si riempie live |

### Configurazioni chiave
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
| Hosting | Firebase — progetto `eugenio-dashboard-2` |
| URL produzione | `https://eugenio-dashboard-2a.web.app/` |
| API target (prod) | `http://51.83.159.175` (da `frontend/.env`) |
| API target (locale) | `http://localhost:5299` (da `frontend/.env.example`) |
| Deploy | GitHub Actions → `firebase-hosting-merge.yml` |

---

## 5. BACKEND WEBAPI

| Parametro | Valore |
|---|---|
| Framework | .NET 9, ASP.NET Core |
| Porta locale | `5299` (HTTP) / `7203` (HTTPS) |
| Porta produzione | `80` via IIS |
| Autenticazione | JWT Bearer |
| Database ORM | Entity Framework Core |
| Realtime | SignalR |

### appsettings.json (valori produzione)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=51.83.159.175,1433;Database=Eugenio-Demo10;User Id=sa3;Password=LionGest123@;Encrypt=False;TrustServerCertificate=True;"
  },
  "Jwt": { "Issuer": "WebApi", "Audience": "WebApiUsers", "ExpirationMinutes": "60" },
  "Admin": { "Username": "admin", "Email": "admin@botdashboard.local" },
  "Smtp": {
    "Host": "smtp.gmail.com", "Port": "587", "EnableSsl": "true",
    "From": "eugeniorosii2025@gmail.com", "Username": "eugeniorosii2025@gmail.com"
  }
}
```

### Endpoint principali
| Endpoint | Metodo | Descrizione |
|---|---|---|
| `/api/Auth/login` | POST | Login utente |
| `/api/Auth/test` | GET | Health check (smoke test deploy) |
| `/api/runtime-mode` | GET/PUT | Legge/imposta Production o Demo |
| `/api/mission/report/range` | GET | Report missioni per periodo |
| `/api/mission/reports/index` | GET | Indice sessioni |
| `/api/admin/users/overview` | GET | Lista utenti con ruoli |
| `/api/admin/user-notification-settings` | GET/PUT | Notifiche email utenti |
| `/api/admin/users/{id}/test-notification-email` | POST | Test invio email |

---

## 6. CI/CD — GITHUB ACTIONS

| Parametro | Valore |
|---|---|
| Repo | `github.com/eugeniorossi2025-sudo/TradingDashboard-2a` |
| Branch principale | `main` |
| Runner self-hosted | `dash2a-backend-runner-01` |
| Runner labels | `self-hosted`, `Windows`, `DASH2A`, `DASH2A-BACKEND` |
| Runner su VPS | `51.83.159.175` |

### Workflows attivi
| File | Trigger | Azione |
|---|---|---|
| `deploy-backend-dash2a.yml` | `workflow_dispatch` manuale | Build + deploy backend su IIS |
| `firebase-hosting-merge.yml` | push su `main` | Deploy frontend Firebase |
| `firebase-hosting-pull-request.yml` | PR | Preview Firebase |

### Segreti GitHub richiesti
| Secret | Uso |
|---|---|
| `DASH2A_RDP_PASSWORD` | Accesso RDP in emergenza |
| `FIREBASE_SERVICE_ACCOUNT_*` | Deploy Firebase |

### Procedura deploy backend
```
1. git push origin main
2. GitHub Actions → deploy-backend-dash2a.yml → Run workflow
3. Inserire: I_UNDERSTAND_BACKEND_DEPLOY_ONLY
4. Verificare smoke test: http://51.83.159.175/api/Auth/test
5. Verificare login: https://eugenio-dashboard-2a.web.app/
```

---

## 7. CREDENZIALI RIEPILOGO

| Servizio | Utente | Note |
|---|---|---|
| OVH VPS Backend RDP | `administrator` | Password in GitHub Secret |
| OVH VPS Decisore RDP | `administrator` | Separato da backend |
| SQL Server Backend | `sa3` | DB `Eugenio-Demo10` su `51.83.159.175` |
| SQL Server Decisore | `sa` | DB `Eugenio-Demo10` su `51.210.181.37` |
| Dashboard Web App admin | `admin` | `https://eugenio-dashboard-2a.web.app/` |
| Gmail SMTP | `eugeniorosii2025@gmail.com` | App password configurata |
| Firebase | `Ak47129898@gmail.com` | Progetto `eugenio-dashboard-2` |
| OVH Account | `eugeniobac2@outlook.it` | Pannello OVH |

---

## 8. AMBIENTE LOCALE SVILUPPO

| Parametro | Valore |
|---|---|
| Backend porta | `http://localhost:5299` |
| Frontend porta | `http://localhost:5001` |
| DB locale | `(localdb)\MSSQLLocalDB`, database `Dash2A_LocalProdLike` |
| Profilo ASP.NET | `LocalProdLike` → `appsettings.LocalProdLike.json` |
| Decisore reale | VPS `51.210.181.37` — HTTP `:5286` interno (non esposto esternamente); SQL `:1433` |
| Decisore RDP | `administrator` — password in GitHub Secret (non committare) |
| Config riferimento | `ops/dash2a-readiness/local-prod-like.env.example` |
| Admin locale | `admin` / `Admin@123456` |
| SMTP | Configurato (Gmail app password) |

### Avvio locale
```powershell
# Dalla root del repo:
.\restart-app-safe.ps1
```

---

## 9. REGOLE OPERATIVE

1. **NON eseguire `dotnet ef database update`** in produzione — le migrazioni EF sono disabilitate.
2. **NON modificare** le tabelle `Users_v2`, `Configurations`, `AspNetRoles` senza review.
3. **NON deployare frontend Firebase** senza che il backend sia già live e funzionante.
4. **NON usare credenziali Dashboard 1** in questo repo.
5. Ogni modifica al DB produzione va preceduta da backup (automatico OVH — snapshot manuale consigliato).
6. Il deploy backend richiede **conferma manuale** nel workflow GitHub Actions.
7. Lo smoke test (`/api/Auth/test` → HTTP 200) è obbligatorio dopo ogni deploy.

---

## 10. CHECKLIST INIZIO SESSIONE

- [ ] Verificare che il runner `dash2a-backend-runner-01` sia online (GitHub → Settings → Actions → Runners)
- [ ] Verificare che `http://51.83.159.175/api/Auth/test` risponda 200
- [ ] Verificare che `https://eugenio-dashboard-2a.web.app/` carichi il frontend
- [ ] Verificare che `git branch` sia su `main` e `git status` sia pulito
- [ ] Leggere le ultime 5 righe di `git log --oneline -5`
