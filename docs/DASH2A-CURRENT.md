# DASH2A CURRENT — 2026-09-25

## Decision and ownership

Canonical repository: eugeniorossi2025-sudo/TradingDashboard-2a.
Dashboard, WebApi and Decisore owner: giacom (docs/AGENT-GIACOM.md).
Gamebot owner: giacomo1 (docs/AGENT-GIACOMO1.md).
The user confirms all prior OVH servers are decommissioned. Treat their IP addresses,
hostnames, runner labels, IIS sites, database instances and deployment records as
historical references only. No legacy OVH endpoint is a current deployment target.

## Verified today

- GitHub Actions repository runners API: exactly one registered runner,
  dash2a-windows-runner-01 (id 23), online; labels self-hosted, Windows, X64, DASH2A.
  There are no old runners registered to delete.
- Runner smoke run 36075924561 passed: Windows service Running, 77.6 GB free on C:.
- Runner inventory run 36076160163: IIS module present; W3SVC, WAS and
  MSSQL$SQLEXPRESS Running; .NET command present. The runner service identity
  cannot read IIS configuration (UnauthorizedAccessException). Old backend
  shared appsettings path, release root and C:\Decisore were absent.
  These observations do not certify an API deployment or its database.
- Firebase frontend https://eugenio-dashboard-2a.web.app/ returned HTTP 200.
  This proves hosting reachability, not frontend-to-API functionality.
- PC4 DNS: tradingdash2a.com and api.tradingdash2a.com both resolve to
  146.59.145.216. HTTPS requests timed out from PC4; target server ownership,
  IIS bindings and certificates remain unverified.
- Legacy hostname vps-b0942869.vps.ovh.net failed DNS lookup. The frontend
  deploy workflow still sets VITE_API_BASE_URL to this dead hostname.
- Last successful Firebase deploy recorded 2026-06-11 (f8ad2b9); last
  successful backend deploy recorded 2026-06-07 (84c8f0e). Those old
  production runs are not evidence of a deployment on the current server.

## Deployment state and next sequence

1. Inventory IIS sites/bindings/pools, Windows identity and installed .NET on
   the current server using an elevated read-only shell. Compare the public IP
   with the DNS A records; avoid printing application secrets.
2. Establish where the production database lives and verify backup and
   application connectivity before modifying any deployed application.
3. Prepare new backend deployment using the current server's runner label
   DASH2A, current IIS paths and shared configuration. Preserve any existing
   applications. Remove hardcoded retired server IPs and credentials from
   active deployment workflow(s), replace with scoped secrets/environment data.
4. Deploy and test WebApi on the current server, then bind and certify
   api.tradingdash2a.com over HTTPS.
5. Change Firebase build-time VITE_API_BASE_URL to the verified HTTPS API,
   deploy frontend to its existing Firebase project, test login and API calls.
   A second frontend server is not needed for this architecture.
6. Decide whether Decisore requires a separate server from measured runtime
   and database requirements. Provision any second OVH server only after its
   role, OS, capacity and recovery path are defined.

Do not run workflows targeting DASH2A-BACKEND or DASH2A-DECISORE until their
runner labels and deployment paths have been explicitly migrated to the new
infrastructure. Do not bulk-delete historical workflows: some contain
hardcoded database credentials and require controlled deactivation and
credential rotation before cleanup.
## Elevated readback from the current server

- Administrator PowerShell `Get-Website`: only Default Web Site, Started,
  %SystemDrive%\inetpub\wwwroot, HTTP *:80:. No DASH2A IIS site or
  HTTPS binding was listed.
- Administrator `sqlcmd -S .\SQLEXPRESS -E -C` reading sys.databases:
  only master, tempdb, model, msdb, all ONLINE. No DASH2A user database.
  The -C flag was needed for the untrusted local SQL certificate.
- The repository contains EF migrations and historical mission exports,
  but the inspected PC4 exports date to May and are not a full current
  production database backup. The historical restore workflow references
  an obsolete server and must not run on the current host.
## Backup search and recovery limit (2026-09-25)

- The user confirms the previous OVH servers are inaccessible. No former
  server can be treated as an available source of database backups.
- PC4 inspection of Downloads, the canonical repo exports, selected Codex
  folders and OneDrive TradingDashboardBackups/Backup-Eugenio did not find
  a current complete DASH2A .bak or .bacpac. The archive under OneDrive
  TradingDashboardBackups contains legacy Dashboard 1 SQL scripts.
  The old disaster recovery note says its OneDrive backup procedure had
  not yet been executed as of 2026-06-06.
- Current OVH runner read-only check of SQL default backup directories and
  C:\Backup(s)/D:\Backup(s) returned no .bak/.bacpac; this scoped check
  cannot prove there are no backups in other storage or the OVH console.
- The repository has SQL Server EF migrations for schema construction;
  InitialEmptyMigration creates identity/config tables, later migrations
  add mission tables. No repository migration or May mission export
  restores all historical customer, accounting and session data.
- Decision needed before a fresh database is made operational: accept
  an empty new database with history unavailable, or first identify a
  verified external backup source. Never run the obsolete restore workflow.
## User clarification — new OVH account

The current OVH console/account is new and does not provide access to
the prior OVH servers or their backups. Pursue a fresh installation
based on repository schema and new SQL data; historical records remain
unavailable absent a separately located full backup. Do not infer
existing customer balances or accounting from May mission exports.
The first installation must use a new server-specific IIS layout and
configuration; the legacy restore workflow is inapplicable.
## Repository build validation

GitHub-hosted Windows build run 36077078784 built backend/WebApi/WebApi.csproj
targeting .NET 9 from the isolated branch: success, zero errors,
38 pre-existing compiler/documentation warnings. Build success does not
verify database schema migrations, IIS hosting bundle, server settings
or deployed API. The first backend deployment attempt is documented below.

## New OVH installation status, 2026-09-25
- BOOTSTRAP_PASS: new IIS site and original local SQL database initialized.
- First API workflow run 36078925395 published and returned local HTTP 200, but its SQL schema gate failed. This is NOT DEPLOY_PASS.
- Read-only runner inventory 36079410369: original new database has only MissionSessions and MissionMarginSamples, no __EFMigrationsHistory and no users table. Preserve it untouched.
- Historical server and EF migrations do not match a fresh current-model installation. On this isolated branch, FreshOvhInitial was generated from the current EF Core 9 model; it creates Users_v2 and MissionSessions. Existing old migrations remain in Git history and in main until an intentional promotion.
- Next gate: Administrator prepares separate Dash2aFresh20260925 database with prepare-fresh-ovh-db.ps1. Verify its output before changing live config or running another deploy. No HTTPS or frontend validation yet.- Administrator confirmed FRESH_DB_PASS: Dash2aFresh20260925 exists with 0 user tables. No schema migration or API configuration switch has run against this fresh database yet.
- Stage step requires stage-fresh-ovh-api.ps1; the deploy workflow then checks the fresh DB name and offline marker before copying anything. The schema gate requires Users_v2, MissionSessions, and the new migration history row. No current OVH deploy pass yet.
## Verified new server backend deploy
- Run 36080279205, commit 2e9ddf6: hosted publish PASS; self-hosted deploy PASS; local /api/Auth/test HTTP 200; schema gate PASS on Dash2aFresh20260925.
- Independent read-only runner run 36080563991: 21 user tables total (20 generated model tables and __EFMigrationsHistory), Users_v2 admin count 1, migration history row count 1, PASS.
- The original partial Eugenio-Demo10 database was not migrated, copied or dropped. Historical customer, accounting and mission data remain unavailable.
- Public HTTPS API, frontend connection and login from outside the server have not yet been verified; local backend deploy PASS does not certify public dashboard readiness.
## Public API and Firebase frontend, 2026-09-25
- Administrator ran setup-api-https-ovh.ps1: win-acme 2.2.9.1701 obtained a Let's Encrypt certificate using HTTP-01, attached HTTPS binding to DASH2A-API, configured renewal task, and opened Windows inbound TCP 443. Contact email was not accepted, but issuance and renewal task completed.
- Independent PC4 readback: https://api.tradingdash2a.com/api/Auth/test HTTP 200; TCP 443 reachable; CORS origin https://eugenio-dashboard-2a.web.app accepted with credentials.
- Firebase Hosting Live run 36081558219 deployed commit 534cfb9: success. Public frontend https://eugenio-dashboard-2a.web.app/ returned 200 and live JS asset contains https://api.tradingdash2a.com and does not contain the retired vps-b0942869 hostname.
- The apex http://tradingdash2a.com/ currently responds with the default IIS Windows Server page; the custom domain is not connected to Firebase. Interactive login and live data operations remain unverified.
