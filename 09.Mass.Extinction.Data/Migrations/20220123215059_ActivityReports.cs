using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ninth.Mass.Extinction.Web.Data.Migrations
{
    public partial class ActivityReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityReports",
                columns: table => new
                {
                    ActivityReportId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initiator = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Args = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Report = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityReports", x => x.ActivityReportId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityReports");
        }
    }
}
