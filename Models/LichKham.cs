using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class LichKham
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaLichKham { get; set; }

    [Required]
    public int MaBenhNhan { get; set; }

    [Required]
    public int MaBacSi { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime NgayKham { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan GioKham { get; set; }

    [StringLength(255)]
    public string? LyDoKham { get; set; }

    public string TrangThai { get; set; } = "ChoXacNhan";

    public DateTime? NgayTao { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("MaBenhNhan")]
    public virtual BenhNhan? BenhNhan { get; set; }

    [ForeignKey("MaBacSi")]
    public virtual NguoiDung? BacSi { get; set; }

    public virtual PhieuKham? PhieuKham { get; set; }
}
