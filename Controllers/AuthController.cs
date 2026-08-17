using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Repositories;

namespace MyMvcApp.Controllers;

public class AuthController : Controller
{
    private readonly IRepository<NguoiDung> _nguoiDungRepository;
    private readonly IRepository<BenhNhan> _benhNhanRepository;

    public AuthController(IRepository<NguoiDung> nguoiDungRepository, IRepository<BenhNhan> benhNhanRepository)
    {
        _nguoiDungRepository = nguoiDungRepository;
        _benhNhanRepository = benhNhanRepository;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.LoaiTaiKhoan == "NguoiDung")
        {
            var users = await _nguoiDungRepository.FindAsync(u => 
                (u.SoDienThoai == model.Username || u.Email == model.Username));
            var user = users.FirstOrDefault();

            if (user != null && user.MatKhau == model.Password)
            {
                if (!user.TrangThai)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                    return View(model);
                }

                await SignInUserAsync(user.MaNguoiDung.ToString(), user.HoTen, user.Role, "NguoiDung");
                return RedirectToLocal(returnUrl);
            }
        }
        else
        {
            var patients = await _benhNhanRepository.FindAsync(p => 
                (p.SoDienThoai == model.Username || p.Email == model.Username));
            var patient = patients.FirstOrDefault();

            if (patient != null && patient.MatKhau == model.Password)
            {
                await SignInUserAsync(patient.MaBenhNhan.ToString(), patient.HoTen, "BenhNhan", "BenhNhan");
                return RedirectToLocal(returnUrl);
            }
        }

        ModelState.AddModelError("", "Tên đăng nhập, mật khẩu hoặc loại tài khoản không chính xác.");
        return View(model);
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            ModelState.AddModelError("", $"Lỗi từ nhà cung cấp: {remoteError}");
            return View("Login");
        }

        var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (info == null || !info.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "Không thể lấy email từ nhà cung cấp mạng xã hội.");
            return View("Login");
        }

        // 1. Kiểm tra bảng NguoiDung
        var users = await _nguoiDungRepository.FindAsync(u => u.Email == email);
        var user = users.FirstOrDefault();
        if (user != null)
        {
            await SignInUserAsync(user.MaNguoiDung.ToString(), user.HoTen, user.Role, "NguoiDung");
            return RedirectToLocal(returnUrl);
        }

        // 2. Kiểm tra bảng BenhNhan
        var patients = await _benhNhanRepository.FindAsync(p => p.Email == email);
        var patient = patients.FirstOrDefault();
        if (patient != null)
        {
            await SignInUserAsync(patient.MaBenhNhan.ToString(), patient.HoTen, "BenhNhan", "BenhNhan");
            return RedirectToLocal(returnUrl);
        }

        // Nếu chưa tồn tại, chuyển hướng sang trang Đăng ký và truyền tên, email
        TempData["ExternalEmail"] = email;
        TempData["ExternalName"] = name;
        TempData["InfoMessage"] = "Vui lòng hoàn tất thông tin đăng ký cho tài khoản mới.";
        return RedirectToAction("Register");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new RegisterViewModel();
        if (TempData["ExternalEmail"] != null)
        {
            model.Email = TempData["ExternalEmail"]?.ToString();
            model.HoTen = TempData["ExternalName"]?.ToString() ?? "";
        }
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Kiểm tra số điện thoại hoặc email đã tồn tại trong bảng Bệnh nhân chưa
        var existingPhone = await _benhNhanRepository.FindAsync(p => p.SoDienThoai == model.SoDienThoai);
        if (existingPhone.Any())
        {
            ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được đăng ký.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.Email))
        {
            var existingEmail = await _benhNhanRepository.FindAsync(p => p.Email == model.Email);
            if (existingEmail.Any())
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
                return View(model);
            }
        }

        if (!string.IsNullOrEmpty(model.CCCD))
        {
            var existingCCCD = await _benhNhanRepository.FindAsync(p => p.CCCD == model.CCCD);
            if (existingCCCD.Any())
            {
                ModelState.AddModelError("CCCD", "CCCD/CMND đã tồn tại trong hệ thống.");
                return View(model);
            }
        }

        var benhNhan = new BenhNhan
        {
            HoTen = model.HoTen,
            NgaySinh = model.NgaySinh,
            GioiTinh = model.GioiTinh,
            SoDienThoai = model.SoDienThoai,
            Email = model.Email,
            MatKhau = model.MatKhau, // Trong thực tế nên Hash mật khẩu
            CCCD = model.CCCD,
            NgayTao = DateTime.Now
        };

        await _benhNhanRepository.AddAsync(benhNhan);
        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(string userId, string name, string role, string userType)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserType", userType)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
