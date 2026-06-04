# Root Owner (console nascosta)

## Deploy DB (PROD, una tantum)

```powershell
sqlcmd -S "51.83.159.175,1434" -d Eugenio-Demo10 -U sa3 -P "<password>" `
  -i ops/dash2a-readiness/root-owner-schema.sql
```

Verifica: `SELECT Id, UserName, IsRootOwner FROM dbo.Users_v2 WHERE IsRootOwner = 1` → **UserId 13**.

Dopo deploy backend: **re-login** UserId 13 per claim JWT `isRootOwner=true`.

## URL (non in menu)

`https://<host>/admin/root-owner`

## Test accettazione

1. Login UserId 13 → `/admin/root-owner` OK  
2. Altro admin → 403 `ROOT_OWNER_ONLY`  
3. Altro admin DELETE/DISABLE User 13 → 403 `ROOT_OWNER_PROTECTED` + riga in `RootOwnerAuditEvents`

## Audit

Tabella `dbo.RootOwnerAuditEvents` — ogni comando root owner e ogni tentativo bloccato su account protetto.
