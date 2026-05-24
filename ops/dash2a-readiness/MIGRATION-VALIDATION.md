# DASH2A Migration Validation

Backend deployment is blocked until EF schema validation is complete.

## Current Finding

`backend/WebApi/Data/AppDbContext.cs` currently maps these new tables:
- `MissionSessions`
- `MissionMarginSamples`
- `UserNotificationSettings`
- `UserAccessEvents`

`backend/WebApi/Migrations/20260524012500_AddMissionReportTables.cs` creates:
- `MissionSessions`
- `MissionMarginSamples`

`backend/WebApi/Migrations/AppDbContextModelSnapshot.cs` does not currently contain the new table names found in `AppDbContext`.

Conclusion: migration/schema must be regenerated or otherwise validated before any server-side deploy. Do not run this migration on production as-is.

## Required Local Validation

Run from repository root:

```powershell
dotnet restore .\backend\WebApi\WebApi.csproj
dotnet build .\backend\WebApi\WebApi.csproj -c Release
```

Generate a migration script for review:

```powershell
dotnet ef migrations script `
  --project .\backend\WebApi\WebApi.csproj `
  --startup-project .\backend\WebApi\WebApi.csproj `
  --idempotent `
  --output .\artifacts\dash2a-migration-idempotent.sql
```

Inspect the script before server use:

```powershell
Select-String -Path .\artifacts\dash2a-migration-idempotent.sql -Pattern "MissionSessions|MissionMarginSamples|UserNotificationSettings|UserAccessEvents|DROP TABLE|DROP COLUMN|ALTER TABLE" -CaseSensitive:$false
```

Expected result:
- All four new tables are represented, or there is a documented reason they already exist.
- No destructive `DROP` operation appears unless explicitly approved.
- The script is idempotent.

## Required Database Preflight

Run only against a read-only or pre-deploy SQL session unless deploy is explicitly approved.

```sql
SELECT DB_NAME() AS CurrentDatabase;

SELECT name
FROM sys.tables
WHERE name IN (
  'MissionSessions',
  'MissionMarginSamples',
  'UserNotificationSettings',
  'UserAccessEvents'
)
ORDER BY name;

SELECT MigrationId, ProductVersion
FROM [__EFMigrationsHistory]
ORDER BY MigrationId;
```

Stop if:
- Connected database is not the intended DASH2A database.
- `__EFMigrationsHistory` is missing or inconsistent.
- Required tables partially exist without a migration record.
- App user lacks rights required for the deploy strategy.

## Required Production Deploy Rule

Production DB migration may happen only after:
- DB backup verified with `RESTORE VERIFYONLY`.
- Migration script reviewed and attached to the deploy record.
- Rollback DB backup path recorded.
- App rollback folder recorded.
- Healthcheck and login smoke test are ready.

## Post-Migration Smoke

Minimum checks after migration in a controlled deploy:

```sql
SELECT COUNT(*) AS MissionSessionsCount FROM dbo.MissionSessions;
SELECT COUNT(*) AS MissionMarginSamplesCount FROM dbo.MissionMarginSamples;
SELECT COUNT(*) AS UserNotificationSettingsCount FROM dbo.UserNotificationSettings;
SELECT COUNT(*) AS UserAccessEventsCount FROM dbo.UserAccessEvents;
```

Application checks:
- WebApi starts without EF exceptions.
- Login works.
- Mission report index endpoint works.
- Admin user access/notification endpoints work.
- Logs contain no SQL permission or missing table errors.

## Approval Gate

Set migration status to approved only when:
- `AppDbContext` and snapshot agree.
- Migration script includes every expected table.
- Script is non-destructive or explicitly approved.
- Backup and rollback are ready.
