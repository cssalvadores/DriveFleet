using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyVehiclePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Vehicles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
