using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DriveFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedVehicleCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Affordable vehicles designed for efficient everyday travel.", "Economy" },
                    { 2, "Compact vehicles suitable for city driving and short trips.", "Compact" },
                    { 3, "Spacious vehicles suitable for families and longer journeys.", "SUV" },
                    { 4, "Premium vehicles offering additional comfort and equipment.", "Luxury" },
                    { 5, "Large vehicles designed for additional passengers or luggage.", "Van" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5);
        }
    }
}
