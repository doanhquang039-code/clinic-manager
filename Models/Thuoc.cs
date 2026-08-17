using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class Thuoc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaThuoc { get; set; }

    [Required]
    [StringLength(100)]
    public string TenThuoc { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string DonViTinh { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [Required]
    public int SoLuongTon { get; set; } = 0;

    [DataType(DataType.Date)]
    public DateTime? HanSuDung { get; set; }

    public string TrangThai { get; set; } = "ConHang";

    // Navigation properties
    public virtual ICollection<ChiTietDonThuoc> ChiTietDonThuocs { get; set; } = new List<ChiTietDonThuoc>();
}
