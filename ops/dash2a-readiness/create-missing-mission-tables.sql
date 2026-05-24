/*
  DASH2A production preparation script - REVIEW ONLY.

  Purpose:
  - Create only the four missing mission/admin support tables in Eugenio-Demo10.
  - Do not touch legacy production tables such as Users, PC, Values, Configurations.

  Required before execution:
  - Full verified backup of production DB.
  - Human review of this script.

  Safety rules:
  - No DROP.
  - No destructive ALTER.
  - No INSERT/UPDATE/DELETE.
  - No FK to legacy Users table because production Users schema differs from local.
*/

SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'[dbo].[MissionSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionSessions] (
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionSessions] PRIMARY KEY,
        [MissionKey] NVARCHAR(128) NULL,
        [StartTime] DATETIME2 NOT NULL,
        [EndTime] DATETIME2 NULL,
        [TotalMargin] DECIMAL(18,2) NOT NULL,
        [RealHandsCount] INT NOT NULL,
        [LastTotalMarginForRealHands] DECIMAL(18,2) NULL,
        [GlobalTarget] DECIMAL(18,2) NOT NULL,
        [ActiveTables] INT NOT NULL,
        [KFactor] DECIMAL(18,2) NOT NULL,
        [RuntimeMode] NVARCHAR(32) NOT NULL,
        [Completed] BIT NOT NULL,
        [ReportPublishedAt] DATETIME2 NULL,
        [FinalizationReason] NVARCHAR(128) NULL,
        [CreatedAt] DATETIME2 NOT NULL
    );
END;
GO

IF OBJECT_ID(N'[dbo].[MissionMarginSamples]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionMarginSamples] (
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionMarginSamples] PRIMARY KEY,
        [SessionId] INT NOT NULL,
        [Timestamp] DATETIME2 NOT NULL,
        [TotalMargin] DECIMAL(18,2) NOT NULL,
        [ActiveTables] INT NOT NULL,
        [VmCurrent] DECIMAL(18,2) NOT NULL,
        [RuntimeMode] NVARCHAR(32) NOT NULL,
        CONSTRAINT [FK_MissionMarginSamples_MissionSessions_SessionId]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[MissionSessions]([ID]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[UserNotificationSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserNotificationSettings] (
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserNotificationSettings] PRIMARY KEY,
        [UserId] INT NOT NULL,
        [NotificationEmail] NVARCHAR(256) NULL,
        [Enabled] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Enabled] DEFAULT(1),
        [Mission] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Mission] DEFAULT(1),
        [System] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_System] DEFAULT(1),
        [Errors] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Errors] DEFAULT(1),
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserNotificationSettings_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [UpdatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserNotificationSettings_UpdatedAtUtc] DEFAULT(SYSUTCDATETIME())
    );
END;
GO

IF OBJECT_ID(N'[dbo].[UserAccessEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserAccessEvents] (
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserAccessEvents] PRIMARY KEY,
        [UserId] INT NULL,
        [Username] NVARCHAR(256) NULL,
        [EventType] NVARCHAR(32) NOT NULL,
        [IpAddress] NVARCHAR(128) NULL,
        [Page] NVARCHAR(512) NULL,
        [UserAgent] NVARCHAR(1024) NULL,
        [OccurredAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserAccessEvents_OccurredAtUtc] DEFAULT(SYSUTCDATETIME())
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionSessions_MissionKey'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionSessions]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_MissionSessions_MissionKey]
        ON [dbo].[MissionSessions]([MissionKey])
        WHERE [MissionKey] IS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionSessions_RuntimeMode'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionSessions]')
)
BEGIN
    CREATE INDEX [IX_MissionSessions_RuntimeMode]
        ON [dbo].[MissionSessions]([RuntimeMode]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionSessions_StartTime'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionSessions]')
)
BEGIN
    CREATE INDEX [IX_MissionSessions_StartTime]
        ON [dbo].[MissionSessions]([StartTime]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionSessions_EndTime'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionSessions]')
)
BEGIN
    CREATE INDEX [IX_MissionSessions_EndTime]
        ON [dbo].[MissionSessions]([EndTime]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionSessions_Completed'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionSessions]')
)
BEGIN
    CREATE INDEX [IX_MissionSessions_Completed]
        ON [dbo].[MissionSessions]([Completed]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionMarginSamples_SessionId'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionMarginSamples]')
)
BEGIN
    CREATE INDEX [IX_MissionMarginSamples_SessionId]
        ON [dbo].[MissionMarginSamples]([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionMarginSamples_Timestamp'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionMarginSamples]')
)
BEGIN
    CREATE INDEX [IX_MissionMarginSamples_Timestamp]
        ON [dbo].[MissionMarginSamples]([Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_MissionMarginSamples_RuntimeMode'
      AND [object_id] = OBJECT_ID(N'[dbo].[MissionMarginSamples]')
)
BEGIN
    CREATE INDEX [IX_MissionMarginSamples_RuntimeMode]
        ON [dbo].[MissionMarginSamples]([RuntimeMode]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_UserNotificationSettings_UserId'
      AND [object_id] = OBJECT_ID(N'[dbo].[UserNotificationSettings]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserNotificationSettings_UserId]
        ON [dbo].[UserNotificationSettings]([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_UserAccessEvents_UserId'
      AND [object_id] = OBJECT_ID(N'[dbo].[UserAccessEvents]')
)
BEGIN
    CREATE INDEX [IX_UserAccessEvents_UserId]
        ON [dbo].[UserAccessEvents]([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_UserAccessEvents_Username'
      AND [object_id] = OBJECT_ID(N'[dbo].[UserAccessEvents]')
)
BEGIN
    CREATE INDEX [IX_UserAccessEvents_Username]
        ON [dbo].[UserAccessEvents]([Username]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_UserAccessEvents_EventType'
      AND [object_id] = OBJECT_ID(N'[dbo].[UserAccessEvents]')
)
BEGIN
    CREATE INDEX [IX_UserAccessEvents_EventType]
        ON [dbo].[UserAccessEvents]([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_UserAccessEvents_OccurredAtUtc'
      AND [object_id] = OBJECT_ID(N'[dbo].[UserAccessEvents]')
)
BEGIN
    CREATE INDEX [IX_UserAccessEvents_OccurredAtUtc]
        ON [dbo].[UserAccessEvents]([OccurredAtUtc]);
END;
GO
