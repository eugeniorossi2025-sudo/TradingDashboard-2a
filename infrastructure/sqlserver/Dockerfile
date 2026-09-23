FROM mcr.microsoft.com/mssql/server:2022-latest

# Railway mounts the persistent data volume as root-owned. SQL Server 2022
# normally starts as uid 10001, which cannot initialize that mount. Running
# the container as root is Microsoft-supported for this storage scenario.
USER root
WORKDIR /var/opt/mssql
