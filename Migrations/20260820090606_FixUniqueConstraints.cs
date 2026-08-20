using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_CCCD",
                table: "BenhNhans");

            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_Email",
                table: "BenhNhans");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_CCCD",
                table: "BenhNhans",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL AND [CCCD] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_Email",
                table: "BenhNhans",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL AND [Email] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_CCCD",
                table: "BenhNhans");

            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_Email",
                table: "BenhNhans");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_CCCD",
                table: "BenhNhans",
                column: "CCCD",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_Email",
                table: "BenhNhans",
                column: "Email",
                unique: true);
        }
    }
}
