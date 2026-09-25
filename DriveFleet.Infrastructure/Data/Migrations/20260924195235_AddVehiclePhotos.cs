using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehiclePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehiclePhotos",
                columns: table => new
                {
                    VehiclePhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiclePhotos", x => x.VehiclePhotoId);
                    table.CheckConstraint("CK_VehiclePhotos_DisplayOrder", "[DisplayOrder] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_VehiclePhotos_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePhotos_VehicleId_DisplayOrder",
                table: "VehiclePhotos",
                columns: new[] { "VehicleId", "DisplayOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehiclePhotos");
        }
    }
}
