using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Số điện thoại hoặc Email")]
    [Display(Name = "Tên đăng nhập (Số điện thoại / Email)")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Loại tài khoản")]
    [Display(Name = "Loại tài khoản")]
    public string LoaiTaiKhoan { get; set; } = "BenhNhan";
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ và Tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Ngày sinh")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn Giới tính")]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = "Nam";

    [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
    [StringLength(15)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
    [DataType(DataType.Password)]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận Mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhanMatKhau { get; set; } = string.Empty;

    [StringLength(12)]
    [Display(Name = "CCCD/CMND")]
    public string? CCCD { get; set; }
}
