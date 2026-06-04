/*
  DASH2A — Root owner schema (idempotent)
  - IsRootOwner on Users_v2
  - RootOwnerAuditEvents
  - TR_Users_v2_RootOwnerProtect

  Bootstrap owner (UserId 13) — adjust if needed:
    UPDATE dbo.Users_v2 SET IsRootOwner = 1 WHERE Id = 13;
*/

SET NOCOUNT ON;

IF COL_LENGTH('dbo.Users_v2', 'IsRootOwner') IS NULL
    ALTER TABLE dbo.Users_v2 ADD IsRootOwner BIT NOT NULL CONSTRAINT DF_Users_v2_IsRootOwner DEFAULT(0);

IF OBJECT_ID(N'[dbo].[RootOwnerAuditEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RootOwnerAuditEvents](
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_RootOwnerAuditEvents] PRIMARY KEY,
        [ActorUserId] INT NULL,
        [ActorUsername] NVARCHAR(256) NULL,
        [Action] NVARCHAR(64) NOT NULL,
        [OccurredAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_RootOwnerAuditEvents_OccurredAtUtc] DEFAULT(SYSUTCDATETIME()),
        [IpAddress] NVARCHAR(128) NULL,
        [UserAgent] NVARCHAR(1024) NULL,
        [Outcome] NVARCHAR(32) NOT NULL,
        [Reason] NVARCHAR(512) NULL,
        [DetailsJson] NVARCHAR(4000) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RootOwnerAuditEvents_OccurredAtUtc' AND object_id = OBJECT_ID(N'[dbo].[RootOwnerAuditEvents]'))
    CREATE INDEX [IX_RootOwnerAuditEvents_OccurredAtUtc] ON [dbo].[RootOwnerAuditEvents]([OccurredAtUtc] DESC);

IF OBJECT_ID(N'[dbo].[TR_Users_v2_RootOwnerProtect]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_Users_v2_RootOwnerProtect];
GO

CREATE TRIGGER [dbo].[TR_Users_v2_RootOwnerProtect]
ON [dbo].[Users_v2]
AFTER DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted d WHERE d.IsRootOwner = 1)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM inserted)
            THROW 51020, 'ROOT_OWNER_PROTECTED: DELETE blocked for root owner.', 1;

        IF EXISTS (
            SELECT 1
            FROM deleted d
            INNER JOIN inserted i ON i.Id = d.Id
            WHERE d.IsRootOwner = 1 AND (
                i.IsRootOwner = 0
                OR ISNULL(i.LockoutEnd, '1900-01-01') > SYSUTCDATETIME()
                OR i.Admin <> d.Admin
                OR ISNULL(i.UserName, '') <> ISNULL(d.UserName, '')
                OR ISNULL(i.Email, '') <> ISNULL(d.Email, '')
                OR ISNULL(i.NormalizedUserName, '') <> ISNULL(d.NormalizedUserName, '')
                OR ISNULL(i.NormalizedEmail, '') <> ISNULL(d.NormalizedEmail, '')
            )
        )
            THROW 51021, 'ROOT_OWNER_PROTECTED: sensitive UPDATE blocked for root owner.', 1;
    END
END;
GO

-- Bootstrap: set only your owner account (default UserId 13 from acceptance tests)
UPDATE dbo.Users_v2 SET IsRootOwner = 1 WHERE Id = 13;

SELECT Id, UserName, Admin, IsRootOwner FROM dbo.Users_v2 WHERE IsRootOwner = 1;
