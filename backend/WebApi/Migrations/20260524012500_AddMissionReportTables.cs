using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionReportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MissionMarginSamples");
            migrationBuilder.DropTable(name: "MissionSessions");
        }
    }
}
