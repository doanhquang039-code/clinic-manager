using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class BenhNhan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaBenhNhan { get; set; }

    [Required]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime NgaySinh { get; set; }

    [Required]
    public string GioiTinh { get; set; } = string.Empty;

    [StringLength(15)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? MatKhau { get; set; }

    [StringLength(12)]
    public string? CCCD { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [StringLength(255)]
    public string? DiUng { get; set; }

    public DateTime? NgayTao { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual ICollection<LichKham> LichKhams { get; set; } = new List<LichKham>();
    public virtual ICollection<PhieuKham> PhieuKhams { get; set; } = new List<PhieuKham>();
}
