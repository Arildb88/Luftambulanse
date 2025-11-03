using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe4NLA.Migrations
{
    /// <inheritdoc />
    public partial class GeoInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeoJson",
                table: "Reports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeoJson",
                table: "Reports");
        }
    }
}
