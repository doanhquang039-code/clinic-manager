using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class ChiTietDichVu
{
    [Key, Column(Order = 0)]
    public int MaPhieuKham { get; set; }

    [Key, Column(Order = 1)]
    public int MaDichVu { get; set; }

    [Required]
    public int SoLuong { get; set; } = 1;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [StringLength(255)]
    public string? KetQua { get; set; }

    // Navigation properties
    [ForeignKey("MaPhieuKham")]
    public virtual PhieuKham? PhieuKham { get; set; }

    [ForeignKey("MaDichVu")]
    public virtual DichVu? DichVu { get; set; }
}
