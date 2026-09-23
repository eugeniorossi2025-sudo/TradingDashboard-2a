FROM mcr.microsoft.com/mssql/server:2022-latest

# Railway attaches the persistent volume as root-owned. Repair its ownership
# before each boot, then run the SQL Server process as its supported mssql user.
USER root
COPY start-sqlserver.sh /usr/local/bin/start-sqlserver.sh
RUN chmod 755 /usr/local/bin/start-sqlserver.sh
ENTRYPOINT ["/usr/local/bin/start-sqlserver.sh"]
