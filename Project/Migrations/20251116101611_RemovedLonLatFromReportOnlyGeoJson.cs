using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe4NLA.Migrations
{
    /// <inheritdoc />
    public partial class RemovedLonLatFromReportOnlyGeoJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Reports");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Reports",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Reports",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Reports",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
