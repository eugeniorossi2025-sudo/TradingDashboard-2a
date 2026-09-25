using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FreshOvhInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    K = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Pos = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.K);
                });

            migrationBuilder.CreateTable(
                name: "ControlRoomCommandOverrides",
                columns: table => new
                {
                    PC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionCode = table.Column<int>(type: "int", nullable: false),
                    CommandType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlRoomCommandOverrides", x => x.PC);
                });

            migrationBuilder.CreateTable(
                name: "MissionSessions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealHandsCount = table.Column<int>(type: "int", nullable: false),
                    LastTotalMarginForRealHands = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GlobalTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveTables = table.Column<int>(type: "int", nullable: false),
                    KFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RuntimeMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    ReportPublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizationReason = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionSessions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Pc",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TOTAL = table.Column<decimal>(type: "decimal(19,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pc", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Pc_CurrentStatus",
                columns: table => new
                {
                    COMPUTER = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KEY_ULTIMO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DT_ULTIMO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ACCOUNT = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TAVOLO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SALDO_INIZIALE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SALDO_ISTANTANEO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MARGINE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MEDIA_ORA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VALORE_GIOCATO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    COLPO_MARTINGALA = table.Column<int>(type: "int", nullable: false),
                    STATO = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    COLORE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CHOSEN_COLOR = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    MAZZO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PBT = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ORE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LAST_UPDATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LAST_ADVICE = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LAST_INFO = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VALUTAZIONE_RISULTATO = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pc_CurrentStatus", x => x.COMPUTER);
                });

            migrationBuilder.CreateTable(
                name: "RootOwnerAuditEvents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    ActorUsername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DetailsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootOwnerAuditEvents", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users_v2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Admin = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRootOwner = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_v2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissionMarginSamples",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveTables = table.Column<int>(type: "int", nullable: false),
                    VmCurrent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RuntimeMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionMarginSamples", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MissionMarginSamples_MissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "MissionSessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    ID = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ID_Command = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    PC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ID_User = table.Column<int>(type: "int", nullable: true),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Bit_Sent = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Commands_Users_v2_ID_User",
                        column: x => x.ID_User,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User_Grid_Configurations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_user = table.Column<int>(type: "int", nullable: false),
                    page_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    grid_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    column_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    display = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Grid_Configurations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_User_Grid_Configurations_Users_v2_ID_user",
                        column: x => x.ID_user,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccessEvents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Page = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessEvents", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserAccessEvents_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationSettings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Mission = table.Column<bool>(type: "bit", nullable: false),
                    System = table.Column<bool>(type: "bit", nullable: false),
                    Errors = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationSettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserNotificationSettings_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPushSubscriptions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPushSubscriptions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserPushSubscriptions_Users_v2_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    ID = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Key = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ID_User = table.Column<int>(type: "int", nullable: true),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Values_Users_v2_ID_User",
                        column: x => x.ID_User,
                        principalTable: "Users_v2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Category",
                table: "ApiLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_CreatedAt",
                table: "ApiLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_Datetime",
                table: "Commands",
                column: "Datetime");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_ID_User",
                table: "Commands",
                column: "ID_User");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_PC",
                table: "Commands",
                column: "PC");

            migrationBuilder.CreateIndex(
                name: "IX_MissionMarginSamples_RuntimeMode",
                table: "MissionMarginSamples",
                column: "RuntimeMode");

            migrationBuilder.CreateIndex(
                name: "IX_MissionMarginSamples_SessionId",
                table: "MissionMarginSamples",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionMarginSamples_Timestamp",
                table: "MissionMarginSamples",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSessions_Completed",
                table: "MissionSessions",
                column: "Completed");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSessions_EndTime",
                table: "MissionSessions",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSessions_MissionKey",
                table: "MissionSessions",
                column: "MissionKey",
                unique: true,
                filter: "[MissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSessions_RuntimeMode",
                table: "MissionSessions",
                column: "RuntimeMode");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSessions_StartTime",
                table: "MissionSessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_RootOwnerAuditEvents_OccurredAtUtc",
                table: "RootOwnerAuditEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_User_Grid_Configurations_ID_user_page_name_grid_name",
                table: "User_Grid_Configurations",
                columns: new[] { "ID_user", "page_name", "grid_name" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEvents_EventType",
                table: "UserAccessEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEvents_OccurredAtUtc",
                table: "UserAccessEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEvents_UserId",
                table: "UserAccessEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEvents_Username",
                table: "UserAccessEvents",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationSettings_UserId",
                table: "UserNotificationSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPushSubscriptions_Endpoint",
                table: "UserPushSubscriptions",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPushSubscriptions_UserId",
                table: "UserPushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users_v2",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users_v2",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Values_ID_User",
                table: "Values",
                column: "ID_User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "ControlRoomCommandOverrides");

            migrationBuilder.DropTable(
                name: "MissionMarginSamples");

            migrationBuilder.DropTable(
                name: "Pc");

            migrationBuilder.DropTable(
                name: "Pc_CurrentStatus");

            migrationBuilder.DropTable(
                name: "RootOwnerAuditEvents");

            migrationBuilder.DropTable(
                name: "User_Grid_Configurations");

            migrationBuilder.DropTable(
                name: "UserAccessEvents");

            migrationBuilder.DropTable(
                name: "UserNotificationSettings");

            migrationBuilder.DropTable(
                name: "UserPushSubscriptions");

            migrationBuilder.DropTable(
                name: "Values");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "MissionSessions");

            migrationBuilder.DropTable(
                name: "Users_v2");
        }
    }
}
