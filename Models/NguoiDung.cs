using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class NguoiDung
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaNguoiDung { get; set; }

    [Required]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(15)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    [StringLength(255)]
    public string MatKhau { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "BacSi";

    public int? MaChuyenKhoa { get; set; }

    public bool TrangThai { get; set; } = true;

    public DateTime? NgayTao { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("MaChuyenKhoa")]
    public virtual ChuyenKhoa? ChuyenKhoa { get; set; }

    public virtual ICollection<LichKham> LichKhams { get; set; } = new List<LichKham>();
    public virtual ICollection<PhieuKham> PhieuKhams { get; set; } = new List<PhieuKham>();
    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}
