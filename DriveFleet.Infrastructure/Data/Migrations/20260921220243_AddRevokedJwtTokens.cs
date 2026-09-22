using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedJwtTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevokedJwtTokens",
                columns: table => new
                {
                    RevokedJwtTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jti = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevokedJwtTokens", x => x.RevokedJwtTokenId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevokedJwtTokens_Jti",
                table: "RevokedJwtTokens",
                column: "Jti",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevokedJwtTokens");
        }
    }
}
