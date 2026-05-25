/*
  DASH2A — production 30-table shell (pre-mission backup schema).
  Generated from production-30-tables-from-backup.txt
  IF NOT EXISTS only — no DROP, no INSERT.
*/
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL
        ,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[ApiConfigurations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApiConfigurations] (
        [id] INT NOT NULL,
        [data] DATETIME2 NOT NULL,
        [pc] NVARCHAR(10) NULL,
        [config] NVARCHAR(4000) NOT NULL
        ,
        CONSTRAINT [PK_ApiConfigurations] PRIMARY KEY ([id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[ApiLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApiLogs] (
        [ID] INT NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL,
        [Action] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
        ,
        CONSTRAINT [PK_ApiLogs] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT NOT NULL,
        [RoleId] INT NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL
        ,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] INT NOT NULL,
        [Name] NVARCHAR(256) NULL,
        [NormalizedName] NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL
        ,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT NOT NULL,
        [UserId] INT NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL
        ,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [ProviderKey] NVARCHAR(450) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] INT NOT NULL
        ,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] INT NOT NULL,
        [RoleId] INT NOT NULL
        ,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[BOMenu]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BOMenu] (
        [id] NUMERIC(18,0) NOT NULL,
        [page_name] VARCHAR(500) NOT NULL,
        [page_title] VARCHAR(1000) NOT NULL,
        [folder] VARCHAR(1000) NULL,
        [id_menu] NUMERIC(18,0) NULL,
        [pos_order] NUMERIC(18,0) NULL,
        [bit_visible] BIT NULL,
        [IsAdmin] BIT NULL,
        [Css] VARCHAR(50) NULL
        ,
        CONSTRAINT [PK_BOMenu] PRIMARY KEY ([id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Commands]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Commands] (
        [ID] NUMERIC(18,0) NOT NULL,
        [ID_Command] NUMERIC(18,0) NULL,
        [PC] VARCHAR(50) NULL,
        [ID_User] INT NULL,
        [Datetime] DATETIME NULL,
        [Bit_Sent] BIT NULL
        ,
        CONSTRAINT [PK_Commands] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Commands_Audit]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Commands_Audit] (
        [ID] NUMERIC(18,0) NOT NULL,
        [ID_Command] NUMERIC(18,0) NULL,
        [PC] VARCHAR(50) NULL,
        [ID_User] INT NULL,
        [Datetime] DATETIME NULL,
        [Bit_Sent] BIT NULL,
        [Tipo] VARCHAR(1) NULL
        ,
        CONSTRAINT [PK_Commands_Audit] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[CommandType]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CommandType] (
        [ID] NUMERIC(18,0) NOT NULL,
        [Description] VARCHAR(50) NULL
        ,
        CONSTRAINT [PK_CommandType] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Configurations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Configurations] (
        [K] NVARCHAR(50) NOT NULL,
        [Description] VARCHAR(500) NULL,
        [Value] NVARCHAR(4000) NULL,
        [Pos] INT NULL
        ,
        CONSTRAINT [PK_Configurations] PRIMARY KEY ([K])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Margini]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Margini] (
        [Id] INT NOT NULL,
        [Margine] DECIMAL(18,0) NULL,
        [Data] DATETIME2 NULL
        ,
        CONSTRAINT [PK_Margini] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Pc]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Pc] (
        [ID] INT NOT NULL,
        [NAME] VARCHAR(50) NULL,
        [TOTAL] DECIMAL(19,0) NULL
        ,
        CONSTRAINT [PK_Pc] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Pc_CurrentStatus]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Pc_CurrentStatus] (
        [COMPUTER] NVARCHAR(50) NOT NULL,
        [KEY_ULTIMO] NUMERIC(18,0) NOT NULL,
        [DT_ULTIMO] DATETIME2 NOT NULL,
        [ACCOUNT] NVARCHAR(500) NULL,
        [TAVOLO] NVARCHAR(500) NULL,
        [SALDO_INIZIALE] DECIMAL(19,0) NOT NULL,
        [SALDO_ISTANTANEO] DECIMAL(19,0) NOT NULL,
        [MARGINE] DECIMAL(19,0) NOT NULL,
        [MEDIA_ORA] DECIMAL(19,0) NOT NULL,
        [VALORE_GIOCATO] DECIMAL(19,0) NOT NULL,
        [COLPO_MARTINGALA] INT NOT NULL,
        [STATO] NVARCHAR(100) NULL,
        [COLORE] NVARCHAR(20) NULL,
        [ALLARME] BIT NOT NULL,
        [MAZZO] NVARCHAR(50) NULL,
        [PBT] NVARCHAR(1) NULL,
        [CHOSEN_COLOR] NVARCHAR(1) NULL,
        [ORE] DECIMAL(10,0) NOT NULL,
        [LAST_UPDATE] DATETIME2 NOT NULL,
        [LAST_ADVICE] NVARCHAR(4000) NULL,
        [LAST_INFO] NVARCHAR(4000) NULL,
        [MISSION_SNAPSHOT] NVARCHAR(4000) NULL,
        [VALUTAZIONE_RISULTATO] NVARCHAR(4000) NULL,
        [CREATED_DATE] DATETIME2 NOT NULL,
        [MARGINE_MIN] DECIMAL(19,0) NULL,
        [MARGINE_MAX] DECIMAL(19,0) NULL,
        [MAZZO_CALCOLATO] NVARCHAR(50) NULL,
        [LAST_ACTION] INT NOT NULL
        ,
        CONSTRAINT [PK_Pc_CurrentStatus] PRIMARY KEY ([COMPUTER])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Pc_CurrentStatus_PBT_History]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Pc_CurrentStatus_PBT_History] (
        [ID] BIGINT NOT NULL,
        [COMPUTER] NVARCHAR(50) NOT NULL,
        [PBT] NVARCHAR(1) NOT NULL,
        [numero_mazzo] NVARCHAR(50) NOT NULL,
        [DT_INSERT] DATETIME2 NOT NULL
        ,
        CONSTRAINT [PK_Pc_CurrentStatus_PBT_History] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Pc_Hours]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Pc_Hours] (
        [ID] INT NOT NULL,
        [PC] VARCHAR(50) NULL,
        [FROMHOUR] DATETIME2 NULL,
        [TOHOUR] DATETIME2 NULL,
        [TOT_HOURS] DECIMAL(19,0) NULL
        ,
        CONSTRAINT [PK_Pc_Hours] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[PC_Start]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PC_Start] (
        [ID] INT NOT NULL,
        [PC] VARCHAR(50) NULL,
        [Key] NUMERIC(18,0) NULL
        ,
        CONSTRAINT [PK_PC_Start] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[SafeGuardJson]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SafeGuardJson] (
        [ID] NUMERIC(18,0) NOT NULL,
        [JSON] NVARCHAR(MAX) NULL,
        [DATETIME] DATETIME NULL,
        [NOTES] VARCHAR(MAX) NULL,
        [MARGINE] VARCHAR(50) NULL
        ,
        CONSTRAINT [PK_SafeGuardJson] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[SettingsJson]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SettingsJson] (
        [ID] NUMERIC(18,0) NOT NULL,
        [JSON] NVARCHAR(MAX) NULL,
        [DATETIME] DATETIME NULL,
        [NOTES] VARCHAR(MAX) NULL
        ,
        CONSTRAINT [PK_SettingsJson] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Statistiche]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Statistiche] (
        [ID] BIGINT NOT NULL,
        [DATA_INIZIO] DATETIME2 NOT NULL,
        [DATA_FINE] DATETIME2 NULL,
        [MARGINE_TOT] DECIMAL(19,0) NOT NULL,
        [MARGINE_MIN] DECIMAL(19,0) NOT NULL,
        [MARGINE_MAX] DECIMAL(19,0) NOT NULL,
        [CREATED_AT] DATETIME2 NOT NULL,
        [ELAPSED] DECIMAL(10,0) NOT NULL,
        [TELEMETRY] NVARCHAR(4000) NULL
        ,
        CONSTRAINT [PK_Statistiche] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Stats]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Stats] (
        [Id] INT NOT NULL,
        [DATETIME] DATETIME NULL,
        [KEY] NUMERIC(18,0) NULL,
        [KEY_ZERO] NUMERIC(18,0) NULL,
        [KEY_INIZIO_TRANSAZIONE] NUMERIC(18,0) NULL,
        [KEY_FINE_TRANSAZIONE] NUMERIC(18,0) NULL,
        [ORE] DECIMAL(19,0) NULL,
        [ACCOUNT] NVARCHAR(500) NULL,
        [COMPUTER] NVARCHAR(500) NULL,
        [TAVOLO] NVARCHAR(500) NULL,
        [SALDO_INIZIALE] DECIMAL(19,0) NULL,
        [SALDO_ISTANTANEO] DECIMAL(19,0) NULL,
        [MARGINE] DECIMAL(19,0) NULL,
        [MEDIA_ORA] DECIMAL(19,0) NULL,
        [STATO] NVARCHAR(500) NULL,
        [COLORE] NVARCHAR(500) NULL,
        [COLPO_MARTINGALA] NVARCHAR(500) NULL,
        [VALORE_GIOCATO] DECIMAL(19,0) NULL,
        [ALLARME] NVARCHAR(500) NULL,
        [MAZZO] NVARCHAR(500) NULL,
        [VINCITA] NVARCHAR(500) NULL
        ,
        CONSTRAINT [PK_Stats] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Stats_Margine]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Stats_Margine] (
        [ID] NUMERIC(18,0) NOT NULL,
        [MARGINE] DECIMAL(19,0) NULL,
        [DATETIME] DATETIME NULL
        ,
        CONSTRAINT [PK_Stats_Margine] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[sysdiagrams]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[sysdiagrams] (
        [name] NVARCHAR(128) NOT NULL,
        [principal_id] INT NOT NULL,
        [diagram_id] INT NOT NULL,
        [version] INT NULL,
        [definition] VARBINARY(MAX) NULL
        ,
        CONSTRAINT [PK_sysdiagrams] PRIMARY KEY ([diagram_id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[User_Grid_Configurations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[User_Grid_Configurations] (
        [ID] INT NOT NULL,
        [ID_user] INT NULL,
        [page_name] VARCHAR(100) NULL,
        [grid_name] VARCHAR(100) NULL,
        [column_name] VARCHAR(100) NULL,
        [display] BIT NULL
        ,
        CONSTRAINT [PK_User_Grid_Configurations] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users] (
        [ID] NUMERIC(18,0) NOT NULL,
        [Description] VARCHAR(50) NULL,
        [Username] VARCHAR(50) NULL,
        [Password] VARCHAR(50) NULL,
        [Administrator] BIT NULL,
        [LastLoginDate] DATETIME NULL
        ,
        CONSTRAINT [PK_Users] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Users_v2]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users_v2] (
        [Id] INT NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Admin] BIT NOT NULL,
        [LastLogin] DATETIME2 NULL,
        [UserName] NVARCHAR(256) NULL,
        [NormalizedUserName] NVARCHAR(256) NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL,
        [TwoFactorEnabled] BIT NOT NULL,
        [LockoutEnd] DATETIMEOFFSET NULL,
        [LockoutEnabled] BIT NOT NULL,
        [AccessFailedCount] INT NOT NULL
        ,
        CONSTRAINT [PK_Users_v2] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Values]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Values] (
        [ID] NUMERIC(18,0) NOT NULL,
        [Key] NUMERIC(18,0) NULL,
        [Description] NVARCHAR(50) NULL,
        [Value] NVARCHAR(50) NULL,
        [ID_User] NUMERIC(18,0) NULL,
        [Datetime] DATETIME NULL
        ,
        CONSTRAINT [PK_Values] PRIMARY KEY ([ID])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Values_Audit]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Values_Audit] (
        [ID] NUMERIC(18,0) NOT NULL,
        [Key] NUMERIC(18,0) NULL,
        [Description] NVARCHAR(50) NULL,
        [Value] NVARCHAR(50) NULL,
        [ID_User] NUMERIC(18,0) NULL,
        [Datetime] DATETIME NULL,
        [Tipo] VARCHAR(1) NULL
        ,
        CONSTRAINT [PK_Values_Audit] PRIMARY KEY ([ID])
    );
END;
GO

