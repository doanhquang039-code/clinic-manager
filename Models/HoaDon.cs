using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class HoaDon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaHoaDon { get; set; }

    [Required]
    public int MaPhieuKham { get; set; }

    [Required]
    public int MaNguoiLap { get; set; }

    public DateTime? NgayLap { get; set; } = DateTime.Now;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TienDichVu { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TienThuoc { get; set; } = 0;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TongTien { get; set; }

    public string PhuongThucThanhToan { get; set; } = "TienMat";

    public string TrangThaiThanhToan { get; set; } = "ChuaThanhToan";

    // Navigation properties
    [ForeignKey("MaPhieuKham")]
    public virtual PhieuKham? PhieuKham { get; set; }

    [ForeignKey("MaNguoiLap")]
    public virtual NguoiDung? NguoiLap { get; set; }
}
