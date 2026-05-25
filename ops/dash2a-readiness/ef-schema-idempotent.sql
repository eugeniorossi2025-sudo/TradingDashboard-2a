-- =============================================================
-- DASH2A - EF Idempotent Migration Script
-- Generato manualmente da migration files del repo
-- Data generazione: 2026-05-25
-- NON applicare senza review e backup confermato
-- =============================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- =============================================================
-- MIGRATION 1: 20251205203847_InitialEmptyMigration
-- Crea: AspNetRoles, Configurations, Logs, PC, Users,
--       AspNetRoleClaims, AspNetUserClaims, AspNetUserLogins,
--       AspNetUserRoles, AspNetUserTokens, Commands,
--       User_Grid_Configurations, Values
-- =============================================================
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251205203847_InitialEmptyMigration')
BEGIN

    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );

    CREATE TABLE [Configurations] (
        [ID] int NOT NULL IDENTITY,
        [Key] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Pos] int NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_Configurations] PRIMARY KEY ([ID])
    );

    CREATE TABLE [Logs] (
        [ID] int NOT NULL IDENTITY,
        [DateTime] datetime2 NOT NULL,
        [Margine] decimal(18,2) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Json] nvarchar(max) NULL,
        CONSTRAINT [PK_Logs] PRIMARY KEY ([ID])
    );

    CREATE TABLE [PC] (
        [PC] nvarchar(100) NOT NULL,
        [Title] nvarchar(255) NULL,
        [STATO] int NOT NULL,
        [IMPORTO] int NOT NULL,
        [LAST_UPDATE] datetime2 NULL,
        CONSTRAINT [PK_PC] PRIMARY KEY ([PC])
    );

    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(max) NULL,
        [Admin] bit NOT NULL,
        [LastLogin] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );

    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [Commands] (
        [ID] int NOT NULL IDENTITY,
        [ID_Command] int NOT NULL,
        [PC] nvarchar(100) NULL,
        [ID_User] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        CONSTRAINT [PK_Commands] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_Commands_Users_ID_User] FOREIGN KEY ([ID_User]) REFERENCES [Users] ([Id]) ON DELETE RESTRICT
    );

    CREATE TABLE [User_Grid_Configurations] (
        [ID] int NOT NULL IDENTITY,
        [ID_user] int NOT NULL,
        [page_name] nvarchar(255) NULL,
        [grid_name] nvarchar(255) NULL,
        [column_name] nvarchar(255) NULL,
        [display] bit NOT NULL,
        CONSTRAINT [PK_User_Grid_Configurations] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_User_Grid_Configurations_Users_ID_user] FOREIGN KEY ([ID_user]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [Values] (
        [ID] int NOT NULL IDENTITY,
        [Key] bigint NOT NULL,
        [Description] nvarchar(255) NULL,
        [Value] nvarchar(max) NULL,
        [Id_User] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [ACCOUNT] nvarchar(100) NULL,
        [TAVOLO] int NULL,
        [MAZZO] int NULL,
        [MARGINE] decimal(18,2) NULL,
        [MEDIA_ORA] decimal(18,2) NULL,
        [STATO] nvarchar(50) NULL,
        [COLORE] nvarchar(50) NULL,
        [COLPO_MARTINGALA] int NULL,
        [VALUTAZIONE] nvarchar(max) NULL,
        [REASON] nvarchar(max) NULL,
        [PREDICTION] nvarchar(100) NULL,
        [PBT] nvarchar(1) NULL,
        [TEMPO] nvarchar(10) NULL,
        CONSTRAINT [PK_Values] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_Values_Users_Id_User] FOREIGN KEY ([Id_User]) REFERENCES [Users] ([Id]) ON DELETE RESTRICT
    );

    -- Indexes
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
    CREATE INDEX [IX_Commands_DateTime] ON [Commands] ([DateTime]);
    CREATE INDEX [IX_Commands_ID_User] ON [Commands] ([ID_User]);
    CREATE INDEX [IX_Commands_PC] ON [Commands] ([PC]);
    CREATE UNIQUE INDEX [IX_Configurations_Key] ON [Configurations] ([Key]);
    CREATE INDEX [IX_Logs_DateTime] ON [Logs] ([DateTime]);
    CREATE INDEX [IX_User_Grid_Configurations_ID_user_page_name_grid_name] ON [User_Grid_Configurations] ([ID_user], [page_name], [grid_name]);
    CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    CREATE INDEX [IX_Values_ACCOUNT_TAVOLO] ON [Values] ([ACCOUNT], [TAVOLO]);
    CREATE INDEX [IX_Values_DateTime] ON [Values] ([DateTime]);
    CREATE INDEX [IX_Values_Id_User] ON [Values] ([Id_User]);
    CREATE INDEX [IX_Values_Key] ON [Values] ([Key]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251205203847_InitialEmptyMigration', N'9.0.0');
END;
GO

-- =============================================================
-- MIGRATION 2: 20251207181749_InitCreateDb
-- Altera: Values.Key da bigint -> nvarchar(450)
-- =============================================================
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251207181749_InitCreateDb')
BEGIN

    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Values]') AND [c].[name] = N'Key');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Values] DROP CONSTRAINT [' + @var0 + '];');

    DROP INDEX [IX_Values_Key] ON [Values];
    ALTER TABLE [Values] ALTER COLUMN [Key] nvarchar(450) NOT NULL;
    CREATE INDEX [IX_Values_Key] ON [Values] ([Key]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251207181749_InitCreateDb', N'9.0.0');
END;
GO

-- =============================================================
-- MIGRATION 3: 20260524012500_AddMissionReportTables
-- Crea: MissionSessions, MissionMarginSamples
-- =============================================================
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260524012500_AddMissionReportTables')
BEGIN

    CREATE TABLE [MissionSessions] (
        [ID] int NOT NULL IDENTITY,
        [MissionKey] nvarchar(128) NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [RealHandsCount] int NOT NULL,
        [LastTotalMarginForRealHands] decimal(18,2) NULL,
        [GlobalTarget] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [KFactor] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        [Completed] bit NOT NULL,
        [ReportPublishedAt] datetime2 NULL,
        [FinalizationReason] nvarchar(128) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MissionSessions] PRIMARY KEY ([ID])
    );

    CREATE TABLE [MissionMarginSamples] (
        [ID] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [VmCurrent] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        CONSTRAINT [PK_MissionMarginSamples] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_MissionMarginSamples_MissionSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [MissionSessions] ([ID]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_MissionMarginSamples_RuntimeMode] ON [MissionMarginSamples] ([RuntimeMode]);
    CREATE INDEX [IX_MissionMarginSamples_SessionId] ON [MissionMarginSamples] ([SessionId]);
    CREATE INDEX [IX_MissionMarginSamples_Timestamp] ON [MissionMarginSamples] ([Timestamp]);
    CREATE INDEX [IX_MissionSessions_Completed] ON [MissionSessions] ([Completed]);
    CREATE INDEX [IX_MissionSessions_EndTime] ON [MissionSessions] ([EndTime]);
    CREATE UNIQUE INDEX [IX_MissionSessions_MissionKey] ON [MissionSessions] ([MissionKey]) WHERE [MissionKey] IS NOT NULL;
    CREATE INDEX [IX_MissionSessions_RuntimeMode] ON [MissionSessions] ([RuntimeMode]);
    CREATE INDEX [IX_MissionSessions_StartTime] ON [MissionSessions] ([StartTime]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524012500_AddMissionReportTables', N'9.0.0');
END;
GO
