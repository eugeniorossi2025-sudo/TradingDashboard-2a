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
