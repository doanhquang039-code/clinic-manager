using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class ChiTietDonThuoc
{
    [Key, Column(Order = 0)]
    public int MaPhieuKham { get; set; }

    [Key, Column(Order = 1)]
    public int MaThuoc { get; set; }

    [Required]
    public int SoLuong { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [StringLength(100)]
    public string? LieuDung { get; set; }

    [StringLength(255)]
    public string? CachDung { get; set; }

    // Navigation properties
    [ForeignKey("MaPhieuKham")]
    public virtual PhieuKham? PhieuKham { get; set; }

    [ForeignKey("MaThuoc")]
    public virtual Thuoc? Thuoc { get; set; }
}
