# Root Owner (console nascosta) — DASH2A

## URL produzione (Firebase live)

| Uso | URL |
|-----|-----|
| **Frontend DASH2A** | https://eugenio-dashboard-2a.web.app/ |
| **Login** | https://eugenio-dashboard-2a.web.app/auth/login |
| **Root Owner** (non in menu) | https://eugenio-dashboard-2a.web.app/admin/root-owner |

**Non usare** `https://eugenio-dashboard-2.web.app/` (senza `a` finale): non è il sito DASH2A live → Firebase **Site Not Found** (HTTP 404).

- Progetto Firebase: `eugenio-dashboard-2a`
- Site ID: `eugenio-dashboard-2a`
- API backend: `https://vps-b0942869.vps.ovh.net`

## Deploy DB (PROD, una tantum)

```powershell
sqlcmd -S "51.83.159.175,1434" -d Eugenio-Demo10 -U sa3 -P "<password>" `
  -i ops/dash2a-readiness/root-owner-schema.sql
```

Verifica: `SELECT Id, UserName, IsRootOwner FROM dbo.Users_v2 WHERE IsRootOwner = 1` → **UserId 13** (Eugenio).

Dopo deploy backend: **re-login** con account root owner per claim JWT `isRootOwner=true`.

## Accesso

- Pagina **non** compare in menu, sidebar o dashboard standard.
- Solo utenti con `IsRootOwner = 1` (bootstrap: UserId 13).
- Altri admin → **403** `ROOT_OWNER_ONLY`.

## Test accettazione

1. Login **Eugenio** (UserId 13) → https://eugenio-dashboard-2a.web.app/admin/root-owner → OK
2. Altro admin → stessa URL → 403 `ROOT_OWNER_ONLY`
3. Altro admin DELETE/DISABLE User 13 → 403 `ROOT_OWNER_PROTECTED` + riga in `RootOwnerAuditEvents`

## Audit

Tabella `dbo.RootOwnerAuditEvents` — ogni comando root owner e ogni tentativo bloccato su account protetto.

Workflow SQL: `.github/workflows/root-owner-bootstrap.yml` (`ROOT_OWNER_VERIFY` / `ROOT_OWNER_BOOTSTRAP`).
