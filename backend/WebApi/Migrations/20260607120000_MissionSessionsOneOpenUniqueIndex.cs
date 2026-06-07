using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class MissionSessionsOneOpenUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_MissionSessions_OneOpen' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
   AND (SELECT COUNT(*) FROM [dbo].[MissionSessions] WHERE [Completed] = 0) <= 1
    CREATE UNIQUE INDEX [UX_MissionSessions_OneOpen] ON [dbo].[MissionSessions]([Completed]) WHERE [Completed] = 0;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_MissionSessions_OneOpen' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    DROP INDEX [UX_MissionSessions_OneOpen] ON [dbo].[MissionSessions];
""");
        }
    }
}
