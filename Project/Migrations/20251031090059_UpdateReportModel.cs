using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe4NLA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DangerType",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "OtherDangerType",
                table: "Reports",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherDangerType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "DangerType",
                table: "Reports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
