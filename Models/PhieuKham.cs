using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class PhieuKham
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuKham { get; set; }

    public int? MaLichKham { get; set; }

    [Required]
    public int MaBenhNhan { get; set; }

    [Required]
    public int MaBacSi { get; set; }

    public DateTime? NgayKham { get; set; } = DateTime.Now;

    [StringLength(500)]
    public string? TrieuChung { get; set; }

    [Required]
    [StringLength(255)]
    public string ChanDoan { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LoiDan { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    // Đánh giá của bệnh nhân
    public int? DanhGia { get; set; }  // 1-5 sao

    [StringLength(500)]
    public string? NhanXet { get; set; }

    // Navigation properties
    [ForeignKey("MaLichKham")]
    public virtual LichKham? LichKham { get; set; }

    [ForeignKey("MaBenhNhan")]
    public virtual BenhNhan? BenhNhan { get; set; }

    [ForeignKey("MaBacSi")]
    public virtual NguoiDung? BacSi { get; set; }

    public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();
    public virtual ICollection<ChiTietDonThuoc> ChiTietDonThuocs { get; set; } = new List<ChiTietDonThuoc>();
    public virtual HoaDon? HoaDon { get; set; }
}
