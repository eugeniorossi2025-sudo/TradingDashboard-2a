#!/usr/bin/env bash
set -euo pipefail

# The service volume is intentionally retained.  This only repairs ownership
# so the non-root SQL Server process can access its own system and data files.
chown -R mssql:root /var/opt/mssql
exec su -s /bin/bash mssql -c '/opt/mssql/bin/sqlservr'
