using Microsoft.EntityFrameworkCore;

namespace MyMvcApp.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<BenhNhan> BenhNhans { get; set; }
    public DbSet<LichKham> LichKhams { get; set; }
    public DbSet<PhieuKham> PhieuKhams { get; set; }
    public DbSet<DichVu> DichVus { get; set; }
    public DbSet<ChiTietDichVu> ChiTietDichVus { get; set; }
    public DbSet<Thuoc> Thuocs { get; set; }
    public DbSet<ChiTietDonThuoc> ChiTietDonThuocs { get; set; }
    public DbSet<HoaDon> HoaDons { get; set; }
    public DbSet<ThongBao> ThongBaos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChiTietDichVu Composite Key
        modelBuilder.Entity<ChiTietDichVu>()
            .HasKey(c => new { c.MaPhieuKham, c.MaDichVu });

        modelBuilder.Entity<ChiTietDichVu>()
            .HasOne(c => c.PhieuKham)
            .WithMany(p => p.ChiTietDichVus)
            .HasForeignKey(c => c.MaPhieuKham)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietDichVu>()
            .HasOne(c => c.DichVu)
            .WithMany(d => d.ChiTietDichVus)
            .HasForeignKey(c => c.MaDichVu)
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietDonThuoc Composite Key
        modelBuilder.Entity<ChiTietDonThuoc>()
            .HasKey(c => new { c.MaPhieuKham, c.MaThuoc });

        modelBuilder.Entity<ChiTietDonThuoc>()
            .HasOne(c => c.PhieuKham)
            .WithMany(p => p.ChiTietDonThuocs)
            .HasForeignKey(c => c.MaPhieuKham)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietDonThuoc>()
            .HasOne(c => c.Thuoc)
            .WithMany(t => t.ChiTietDonThuocs)
            .HasForeignKey(c => c.MaThuoc)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints
        modelBuilder.Entity<NguoiDung>()
            .HasIndex(u => u.SoDienThoai).IsUnique();
        modelBuilder.Entity<NguoiDung>()
            .HasIndex(u => u.Email).IsUnique();
            
        modelBuilder.Entity<BenhNhan>()
            .HasIndex(b => b.SoDienThoai).IsUnique();
        modelBuilder.Entity<BenhNhan>()
            .HasIndex(b => b.Email).IsUnique().HasFilter("[Email] IS NOT NULL AND [Email] <> ''");
        modelBuilder.Entity<BenhNhan>()
            .HasIndex(b => b.CCCD).IsUnique().HasFilter("[CCCD] IS NOT NULL AND [CCCD] <> ''");

        modelBuilder.Entity<LichKham>()
            .HasIndex(l => new { l.MaBacSi, l.NgayKham, l.GioKham }).IsUnique();

        modelBuilder.Entity<PhieuKham>()
            .HasIndex(p => p.MaLichKham).IsUnique();

        modelBuilder.Entity<HoaDon>()
            .HasIndex(h => h.MaPhieuKham).IsUnique();

        // Foreign Key behaviors as defined in script
        modelBuilder.Entity<NguoiDung>()
            .HasOne(n => n.ChuyenKhoa)
            .WithMany(c => c.NguoiDungs)
            .HasForeignKey(n => n.MaChuyenKhoa)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<LichKham>()
            .HasOne(l => l.BenhNhan)
            .WithMany(b => b.LichKhams)
            .HasForeignKey(l => l.MaBenhNhan)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LichKham>()
            .HasOne(l => l.BacSi)
            .WithMany(n => n.LichKhams)
            .HasForeignKey(l => l.MaBacSi)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PhieuKham>()
            .HasOne(p => p.LichKham)
            .WithOne(l => l.PhieuKham)
            .HasForeignKey<PhieuKham>(p => p.MaLichKham)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PhieuKham>()
            .HasOne(p => p.BenhNhan)
            .WithMany(b => b.PhieuKhams)
            .HasForeignKey(p => p.MaBenhNhan)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PhieuKham>()
            .HasOne(p => p.BacSi)
            .WithMany(n => n.PhieuKhams)
            .HasForeignKey(p => p.MaBacSi)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.PhieuKham)
            .WithOne(p => p.HoaDon)
            .HasForeignKey<HoaDon>(h => h.MaPhieuKham)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.NguoiLap)
            .WithMany(n => n.HoaDons)
            .HasForeignKey(h => h.MaNguoiLap)
            .OnDelete(DeleteBehavior.Restrict);

        // Seeding Data
        modelBuilder.Entity<ChuyenKhoa>().HasData(
            new ChuyenKhoa { MaChuyenKhoa = 1, TenChuyenKhoa = "Khoa Nội", MoTa = "Chuyên khám và điều trị các bệnh nội khoa" },
            new ChuyenKhoa { MaChuyenKhoa = 2, TenChuyenKhoa = "Khoa Ngoại", MoTa = "Chuyên phẫu thuật và điều trị các bệnh ngoại khoa" },
            new ChuyenKhoa { MaChuyenKhoa = 3, TenChuyenKhoa = "Nhi khoa", MoTa = "Chuyên khám và điều trị bệnh cho trẻ em" },
            new ChuyenKhoa { MaChuyenKhoa = 4, TenChuyenKhoa = "Sản phụ khoa", MoTa = "Chuyên khám và điều trị các bệnh phụ khoa, thai sản" },
            new ChuyenKhoa { MaChuyenKhoa = 5, TenChuyenKhoa = "Nha khoa", MoTa = "Chuyên khám và điều trị các bệnh răng hàm mặt" },
            new ChuyenKhoa { MaChuyenKhoa = 6, TenChuyenKhoa = "Da liễu", MoTa = "Chuyên khám và điều trị các bệnh về da" }
        );

        modelBuilder.Entity<NguoiDung>().HasData(
            new NguoiDung { MaNguoiDung = 1, HoTen = "Quản trị viên Hệ thống", SoDienThoai = "0999999999", Email = "admin@medicore.com", MatKhau = "123456", Role = "Admin", TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 2, HoTen = "Giám đốc Điều hành", SoDienThoai = "0988888888", Email = "manager@medicore.com", MatKhau = "123456", Role = "Manager", TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 3, HoTen = "Lễ Tân Số 1", SoDienThoai = "0977777777", Email = "letan1@medicore.com", MatKhau = "123456", Role = "LeTan", TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 4, HoTen = "Lễ Tân Số 2", SoDienThoai = "0966666666", Email = "letan2@medicore.com", MatKhau = "123456", Role = "LeTan", TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 5, HoTen = "BS. Nguyễn Minh Khoa", SoDienThoai = "0911111111", Email = "khoanm@medicore.com", MatKhau = "123456", Role = "BacSi", MaChuyenKhoa = 1, TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 6, HoTen = "ThS.BS. Trần Hữu Lan", SoDienThoai = "0922222222", Email = "lanth@medicore.com", MatKhau = "123456", Role = "BacSi", MaChuyenKhoa = 2, TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 7, HoTen = "BS.CKII. Lê Quốc Minh", SoDienThoai = "0933333333", Email = "minhlq@medicore.com", MatKhau = "123456", Role = "BacSi", MaChuyenKhoa = 3, TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 8, HoTen = "BS. Phạm Hoàng Phúc", SoDienThoai = "0944444444", Email = "phucph@medicore.com", MatKhau = "123456", Role = "BacSi", MaChuyenKhoa = 4, TrangThai = true, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 9, HoTen = "BS. Võ Thị Tuyết", SoDienThoai = "0955555555", Email = "tuyetvt@medicore.com", MatKhau = "123456", Role = "BacSi", MaChuyenKhoa = 5, TrangThai = false, NgayTao = new DateTime(2026, 1, 1) },
            new NguoiDung { MaNguoiDung = 10, HoTen = "Thu Ngân 1", SoDienThoai = "0900000000", Email = "thungan@medicore.com", MatKhau = "123456", Role = "ThuNgan", TrangThai = true, NgayTao = new DateTime(2026, 1, 1) }
        );

        modelBuilder.Entity<BenhNhan>().HasData(
            new BenhNhan { MaBenhNhan = 1, HoTen = "Bệnh Nhân VIP", NgaySinh = new DateTime(1990, 1, 1), GioiTinh = "Nam", SoDienThoai = "0811111111", Email = "bn.vip@gmail.com", MatKhau = "123456", CCCD = "079090001111", DiaChi = "Hà Nội", NgayTao = new DateTime(2026, 1, 1) },
            new BenhNhan { MaBenhNhan = 2, HoTen = "Bệnh Nhân Tiêu Chuẩn", NgaySinh = new DateTime(1995, 5, 15), GioiTinh = "Nữ", SoDienThoai = "0822222222", Email = "bn.normal@gmail.com", MatKhau = "123456", CCCD = "079095002222", DiaChi = "TP.HCM", DiUng = "Hải sản", NgayTao = new DateTime(2026, 1, 1) }
        );
    }
}
