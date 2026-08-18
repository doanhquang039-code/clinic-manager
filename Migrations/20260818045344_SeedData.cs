using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BenhNhans",
                columns: new[] { "MaBenhNhan", "CCCD", "DiUng", "DiaChi", "Email", "GioiTinh", "HoTen", "MatKhau", "NgaySinh", "NgayTao", "SoDienThoai" },
                values: new object[,]
                {
                    { 1, "079090001111", null, "Hà Nội", "bn.vip@gmail.com", "Nam", "Bệnh Nhân VIP", "123456", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "0811111111" },
                    { 2, "079095002222", "Hải sản", "TP.HCM", "bn.normal@gmail.com", "Nữ", "Bệnh Nhân Tiêu Chuẩn", "123456", new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "0822222222" }
                });

            migrationBuilder.InsertData(
                table: "ChuyenKhoas",
                columns: new[] { "MaChuyenKhoa", "MoTa", "TenChuyenKhoa" },
                values: new object[,]
                {
                    { 1, "Chuyên khám và điều trị các bệnh nội khoa", "Khoa Nội" },
                    { 2, "Chuyên phẫu thuật và điều trị các bệnh ngoại khoa", "Khoa Ngoại" },
                    { 3, "Chuyên khám và điều trị bệnh cho trẻ em", "Nhi khoa" },
                    { 4, "Chuyên khám và điều trị các bệnh phụ khoa, thai sản", "Sản phụ khoa" },
                    { 5, "Chuyên khám và điều trị các bệnh răng hàm mặt", "Nha khoa" },
                    { 6, "Chuyên khám và điều trị các bệnh về da", "Da liễu" }
                });

            migrationBuilder.InsertData(
                table: "NguoiDungs",
                columns: new[] { "MaNguoiDung", "Email", "HoTen", "MaChuyenKhoa", "MatKhau", "NgayTao", "Role", "SoDienThoai", "TrangThai" },
                values: new object[,]
                {
                    { 1, "admin@medicore.com", "Quản trị viên Hệ thống", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", "0999999999", true },
                    { 2, "manager@medicore.com", "Giám đốc Điều hành", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Manager", "0988888888", true },
                    { 3, "letan1@medicore.com", "Lễ Tân Số 1", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LeTan", "0977777777", true },
                    { 4, "letan2@medicore.com", "Lễ Tân Số 2", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LeTan", "0966666666", true },
                    { 5, "khoanm@medicore.com", "BS. Nguyễn Minh Khoa", 1, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0911111111", true },
                    { 6, "lanth@medicore.com", "ThS.BS. Trần Hữu Lan", 2, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0922222222", true },
                    { 7, "minhlq@medicore.com", "BS.CKII. Lê Quốc Minh", 3, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0933333333", true },
                    { 8, "phucph@medicore.com", "BS. Phạm Hoàng Phúc", 4, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0944444444", true },
                    { 9, "tuyetvt@medicore.com", "BS. Võ Thị Tuyết", 5, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0955555555", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BenhNhans",
                keyColumn: "MaBenhNhan",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BenhNhans",
                keyColumn: "MaBenhNhan",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "NguoiDungs",
                keyColumn: "MaNguoiDung",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ChuyenKhoas",
                keyColumn: "MaChuyenKhoa",
                keyValue: 5);
        }
    }
}
