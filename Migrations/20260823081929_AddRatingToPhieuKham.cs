using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToPhieuKham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DanhGia",
                table: "PhieuKhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanXet",
                table: "PhieuKhams",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhGia",
                table: "PhieuKhams");

            migrationBuilder.DropColumn(
                name: "NhanXet",
                table: "PhieuKhams");
        }
    }
}
