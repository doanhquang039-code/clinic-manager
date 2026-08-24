using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenhNhans",
                columns: table => new
                {
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DiUng = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhans", x => x.MaBenhNhan);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenKhoas",
                columns: table => new
                {
                    MaChuyenKhoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuyenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenKhoas", x => x.MaChuyenKhoa);
                });

            migrationBuilder.CreateTable(
                name: "DichVus",
                columns: table => new
                {
                    MaDichVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDichVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVus", x => x.MaDichVu);
                });

            migrationBuilder.CreateTable(
                name: "Thuocs",
                columns: table => new
                {
                    MaThuoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    HanSuDung = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thuocs", x => x.MaThuoc);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaChuyenKhoa = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDungs_ChuyenKhoas_MaChuyenKhoa",
                        column: x => x.MaChuyenKhoa,
                        principalTable: "ChuyenKhoas",
                        principalColumn: "MaChuyenKhoa",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LichKhams",
                columns: table => new
                {
                    MaLichKham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false),
                    MaBacSi = table.Column<int>(type: "int", nullable: false),
                    NgayKham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioKham = table.Column<TimeSpan>(type: "time", nullable: false),
                    LyDoKham = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichKhams", x => x.MaLichKham);
                    table.ForeignKey(
                        name: "FK_LichKhams_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichKhams_NguoiDungs_MaBacSi",
                        column: x => x.MaBacSi,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuChis",
                columns: table => new
                {
                    MaPhieuChi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiLap = table.Column<int>(type: "int", nullable: false),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: true),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoaiPhieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuChis", x => x.MaPhieuChi);
                    table.ForeignKey(
                        name: "FK_PhieuChis_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan");
                    table.ForeignKey(
                        name: "FK_PhieuChis_NguoiDungs_MaNguoiLap",
                        column: x => x.MaNguoiLap,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBaos_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuKhams",
                columns: table => new
                {
                    MaPhieuKham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLichKham = table.Column<int>(type: "int", nullable: true),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false),
                    MaBacSi = table.Column<int>(type: "int", nullable: false),
                    NgayKham = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrieuChung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChanDoan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoiDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DanhGia = table.Column<int>(type: "int", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuKhams", x => x.MaPhieuKham);
                    table.ForeignKey(
                        name: "FK_PhieuKhams_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuKhams_LichKhams_MaLichKham",
                        column: x => x.MaLichKham,
                        principalTable: "LichKhams",
                        principalColumn: "MaLichKham",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuKhams_NguoiDungs_MaBacSi",
                        column: x => x.MaBacSi,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDichVus",
                columns: table => new
                {
                    MaPhieuKham = table.Column<int>(type: "int", nullable: false),
                    MaDichVu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    KetQua = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDichVus", x => new { x.MaPhieuKham, x.MaDichVu });
                    table.ForeignKey(
                        name: "FK_ChiTietDichVus_DichVus_MaDichVu",
                        column: x => x.MaDichVu,
                        principalTable: "DichVus",
                        principalColumn: "MaDichVu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVus_PhieuKhams_MaPhieuKham",
                        column: x => x.MaPhieuKham,
                        principalTable: "PhieuKhams",
                        principalColumn: "MaPhieuKham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonThuocs",
                columns: table => new
                {
                    MaPhieuKham = table.Column<int>(type: "int", nullable: false),
                    MaThuoc = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    LieuDung = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CachDung = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonThuocs", x => new { x.MaPhieuKham, x.MaThuoc });
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuocs_PhieuKhams_MaPhieuKham",
                        column: x => x.MaPhieuKham,
                        principalTable: "PhieuKhams",
                        principalColumn: "MaPhieuKham",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuocs_Thuocs_MaThuoc",
                        column: x => x.MaThuoc,
                        principalTable: "Thuocs",
                        principalColumn: "MaThuoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuKham = table.Column<int>(type: "int", nullable: false),
                    MaNguoiLap = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TienDichVu = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TienThuoc = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_NguoiDungs_MaNguoiLap",
                        column: x => x.MaNguoiLap,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDons_PhieuKhams_MaPhieuKham",
                        column: x => x.MaPhieuKham,
                        principalTable: "PhieuKhams",
                        principalColumn: "MaPhieuKham",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    { 10, "thungan@medicore.com", "Thu Ngân 1", null, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ThuNgan", "0900000000", true },
                    { 5, "khoanm@medicore.com", "BS. Nguyễn Minh Khoa", 1, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0911111111", true },
                    { 6, "lanth@medicore.com", "ThS.BS. Trần Hữu Lan", 2, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0922222222", true },
                    { 7, "minhlq@medicore.com", "BS.CKII. Lê Quốc Minh", 3, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0933333333", true },
                    { 8, "phucph@medicore.com", "BS. Phạm Hoàng Phúc", 4, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0944444444", true },
                    { 9, "tuyetvt@medicore.com", "BS. Võ Thị Tuyết", 5, "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BacSi", "0955555555", false }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_SoDienThoai",
                table: "BenhNhans",
                column: "SoDienThoai",
                unique: true,
                filter: "[SoDienThoai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVus_MaDichVu",
                table: "ChiTietDichVus",
                column: "MaDichVu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonThuocs_MaThuoc",
                table: "ChiTietDonThuocs",
                column: "MaThuoc");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaNguoiLap",
                table: "HoaDons",
                column: "MaNguoiLap");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaPhieuKham",
                table: "HoaDons",
                column: "MaPhieuKham",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichKhams_MaBacSi_NgayKham_GioKham",
                table: "LichKhams",
                columns: new[] { "MaBacSi", "NgayKham", "GioKham" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichKhams_MaBenhNhan",
                table: "LichKhams",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_Email",
                table: "NguoiDungs",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_MaChuyenKhoa",
                table: "NguoiDungs",
                column: "MaChuyenKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_SoDienThoai",
                table: "NguoiDungs",
                column: "SoDienThoai",
                unique: true,
                filter: "[SoDienThoai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuChis_MaBenhNhan",
                table: "PhieuChis",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuChis_MaNguoiLap",
                table: "PhieuChis",
                column: "MaNguoiLap");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKhams_MaBacSi",
                table: "PhieuKhams",
                column: "MaBacSi");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKhams_MaBenhNhan",
                table: "PhieuKhams",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKhams_MaLichKham",
                table: "PhieuKhams",
                column: "MaLichKham",
                unique: true,
                filter: "[MaLichKham] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_MaNguoiDung",
                table: "ThongBaos",
                column: "MaNguoiDung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDichVus");

            migrationBuilder.DropTable(
                name: "ChiTietDonThuocs");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "PhieuChis");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "DichVus");

            migrationBuilder.DropTable(
                name: "Thuocs");

            migrationBuilder.DropTable(
                name: "PhieuKhams");

            migrationBuilder.DropTable(
                name: "LichKhams");

            migrationBuilder.DropTable(
                name: "BenhNhans");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "ChuyenKhoas");
        }
    }
}
