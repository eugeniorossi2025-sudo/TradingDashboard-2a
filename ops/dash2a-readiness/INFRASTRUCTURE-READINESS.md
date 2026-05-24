# DASH2A Infrastructure Readiness

This document is the gate before any DASH2A merge, Firebase deploy, or backend deploy.

## Current Gate Status

Status: BLOCKED for deploy.

Reasons:
- Backend server readiness is not verified.
- IIS/App Pool/service and deployment folders are not identified.
- App and DB backups are not verified.
- Rollback is not tested.
- Backend migrations are not validated server-side.
- Repository still contains versioned secrets in application settings.
- Firebase live deploy is automatic on merge to `main` if the Firebase service account secret exists.

## Known Operational Map

| Item | Current value | Status |
| --- | --- | --- |
| Repository | `https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git` | Known |
| PR | `#1`, `feature/mobile-financial-reports` -> `main` | Known |
| Public frontend | `https://eugenio-dashboard-2a.web.app/auth/login?redirect=/pages/user` | Known |
| Firebase project | `eugenio-dashboard-2` | Known |
| Backend/API target used by frontend workflows | `http://51.83.159.175` | Known, readiness not verified |
| WebApi SQL host | `51.83.159.175:1433` | Known, access not fully validated |
| Decisore SQL host | `51.210.181.37:1433` | Known, access not fully validated |
| Local frontend | `http://localhost:5001` | Known |
| Local WebApi | `http://localhost:5299` | Known |
| OVH service name | `Back-end Dashboard` | Known from OVH panel |
| OVH VPS hostname | `vps-4ca306e8.vps.ovh.net` | Known from OVH panel |
| OVH server status | `Active`, boot `LOCAL` | Known from OVH panel |
| OVH server OS | `Windows Server 2025 Standard (Desktop)` | Known from OVH panel |
| OVH region/location | `os-waw2`, Warsaw (WAW), Poland | Known from OVH panel |
| OVH plan | `VPS-2`, 6 vCore, 12 GB RAM, 100 GB storage | Known from OVH panel |
| OVH IPv4 | `51.83.159.175` | Known from OVH panel |
| OVH IPv6 | `2001:41d0:601:1100::80ee` | Known from OVH panel |
| OVH snapshot | Disabled | Known from OVH panel |
| OVH automatic backup | Standard, last backup `2026-05-23 21:17` | Known from OVH panel; restore not tested |
| RDP credential secret | GitHub Actions secret `DASH2A_RDP_PASSWORD` | Available as prerequisite; do not print or use automatically |
| IIS site/App Pool or service | Unknown | Must identify |
| Backend deploy folder | Unknown | Must identify |
| Active app backup folder | Unknown | Must identify |
| DB backup target folder | Unknown | Must identify |
| Healthcheck URL | Unknown | Must verify |

## Stop Conditions

Stop immediately if any of these are true:
- The server is not confirmed as DASH2A.
- The deployment folder or process/App Pool/service is unknown.
- A DB backup cannot be produced and verified.
- An app backup cannot be produced and verified.
- Environment variables/secrets are missing.
- `appsettings.json` still contains real production secrets at merge time.
- Migration script does not match the final `DbContext`.
- Firebase project is not `eugenio-dashboard-2`.
- Any Dashboard 1 project, URL, SQL login, or Firebase project is selected.

## Server Audit Checklist

Run only after the user explicitly authorizes server access.

The first server pass must be read-only. Do not restart services, edit files, run migrations, deploy artifacts, or change firewall/IIS settings during this pass.

Record these fields before any change:
- Server provider/account: `OVHcloud VPS / Back-end Dashboard`
- Host/IP: `vps-4ca306e8.vps.ovh.net` / `51.83.159.175`
- OS/version: `Windows Server 2025 Standard (Desktop)`
- Web server: `TODO`
- Process model: `IIS App Pool` / `Windows Service` / `systemd` / other
- Site name: `TODO`
- App Pool/service name: `TODO`
- App root/current folder: `TODO`
- Logs folder: `TODO`
- Backup root: `TODO`
- DB engine/version: `TODO`
- DB name for WebApi: `TODO`
- DB user used by app: `TODO`
- Healthcheck endpoint: `TODO`

OVH panel backup status:
- Snapshot: disabled.
- Automatic backup: Standard, last reported backup `2026-05-23 21:17`.
- Restore procedure: not tested.
- App-level and DB-level backups: still required before deploy.

Remote access prerequisite:
- RDP target: `51.83.159.175`.
- RDP user: `administrator`.
- RDP password is stored as GitHub Actions secret `DASH2A_RDP_PASSWORD`.
- Do not print, echo, export, or write the secret to logs.
- Do not use the secret for automated deploys until a protected workflow/runbook is explicitly approved.
- Manual/read-only discovery remains the only approved server access mode at this stage.

## Read-Only Server Access Checklist

Use this checklist to prepare the first authorized server session. The goal is identification only.

Before connecting:
- Confirm the server is DASH2A, not Dashboard 1.
- Confirm the access method: RDP, SSH, hosting panel, or SQL-only.
- Confirm who is authorizing access.
- Confirm whether commands may be copied from the runbook or only observed manually.
- Confirm no restart, deploy, DB migration, or config edit is allowed.
- Prepare a transcript/log location for findings.

If Windows/IIS:
- Record Windows version.
- Record IIS installed/version.
- List IIS sites without changing them.
- List App Pools without recycling them.
- Identify the site bound to the DASH2A host/IP.
- Identify physical path for the app.
- Identify `web.config` path if present.
- Identify logs folder.
- Identify how environment variables are set: App Pool env, `web.config`, machine env, hosting panel, or other.

If Linux/systemd:
- Record OS version.
- List candidate services without restarting.
- Identify listening ports.
- Identify reverse proxy config, if any.
- Identify app folder and service unit file.
- Identify env file path and logs.

SQL/read-only:
- Confirm SQL host and database.
- Confirm login used by app.
- Check database access only.
- List tables and EF migration history only.
- Do not run backup, restore, migration, or DDL in the first audit pass.

Minimum output to capture:
- Server identity.
- App process name.
- App folder.
- Env/secrets source.
- DB name and access status.
- Healthcheck URL and current response.
- Open blockers.

## Required Server Secrets

These must live in server environment configuration or a secret manager, not in source control.

WebApi:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpirationMinutes`
- `Admin__Username`
- `Admin__Password`
- `Admin__Email`
- `Smtp__Host`
- `Smtp__Port`
- `Smtp__EnableSsl`
- `Smtp__Username`
- `Smtp__Password`
- `Smtp__From`
- `ApplicationInsights__ConnectionString` if used

Decision engine:
- `ConnectionStrings__DefaultConnection`

Frontend/Firebase:
- `VITE_API_BASE_URL`
- GitHub secret `FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2`

## Versioned Secret Findings

The following files currently contain real or secret-like values and must be cleaned before merge/deploy:
- `backend/WebApi/appsettings.json`
- `decision-engine/Decisore/appsettings.json`
- `tools/eugenio-gamebot/source/Gamebot/app.config`
- `tools/eugenio-gamebot/release-reference/Gamebot.exe.config`
- `tools/eugenio-bot/Gamebot/app.config`
- `restart-app-safe.ps1` contains a local smoke-test admin password value.

Cleanup rule:
- Replace production values with placeholders or remove them from tracked files.
- Put actual values only in server env vars, GitHub secrets, or local untracked config.
- Rotate any real credential that has been committed.

## Readiness Sign-Off

Do not merge or deploy until every row is complete.

| Check | Owner | Status |
| --- | --- | --- |
| Server identity confirmed | TODO | Blocked |
| App folder identified | TODO | Blocked |
| App process/App Pool identified | TODO | Blocked |
| DB backup verified | TODO | Blocked |
| App backup verified | TODO | Blocked |
| Rollback rehearsed | TODO | Blocked |
| Secrets removed from repo | TODO | Blocked |
| Server env configured | TODO | Blocked |
| Migration validated | TODO | Blocked |
| Firebase workflow reviewed | TODO | Blocked |
| Backend deploy trigger reviewed | TODO | Blocked |
