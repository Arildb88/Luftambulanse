using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe4NLA.Migrations
{
    /// <inheritdoc />
    public partial class AddHeightAndLeightColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AreLighted",
                table: "Reports",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "HeightInnMeters",
                table: "Reports",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreLighted",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "HeightInnMeters",
                table: "Reports");
        }
    }
}
