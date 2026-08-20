using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NguoiDungs",
                columns: new[] { "MaNguoiDung", "Email", "HoTen", "MaChuyenKhoa", "MatKhau", "NgayTao", "Role", "SoDienThoai", "TrangThai" },
                values: new object[] { 10, "thungan@medicore.com", "Thu Ngân 1", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ThuNgan", "0900000000", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 10);
        }
    }
}
