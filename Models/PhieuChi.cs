using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class PhieuChi
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuChi { get; set; }

    [Required]
    public int MaNguoiLap { get; set; }

    public int? MaBenhNhan { get; set; }

    public DateTime NgayLap { get; set; } = DateTime.Now;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal SoTien { get; set; } = 0;

    [Required]
    [StringLength(255)]
    public string LyDo { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LoaiPhieu { get; set; } = "ChiKhac"; // "HoanTien", "NhapThuoc", "ChiKhac"

    // Navigation properties
    [ForeignKey("MaNguoiLap")]
    public virtual NguoiDung? NguoiLap { get; set; }

    [ForeignKey("MaBenhNhan")]
    public virtual BenhNhan? BenhNhan { get; set; }
}
