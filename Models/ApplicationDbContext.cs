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
            .HasIndex(b => b.Email).IsUnique();
        modelBuilder.Entity<BenhNhan>()
            .HasIndex(b => b.CCCD).IsUnique();

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
    }
}
