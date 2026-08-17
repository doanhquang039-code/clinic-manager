using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models;

public class ChuyenKhoa
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaChuyenKhoa { get; set; }

    [Required]
    [StringLength(100)]
    public string TenChuyenKhoa { get; set; } = string.Empty;

    [StringLength(255)]
    public string? MoTa { get; set; }

    // Navigation properties
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
