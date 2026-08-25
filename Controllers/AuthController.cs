using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

                await SignInUserAsync(user.MaNguoiDung.ToString(), user.HoTen, user.Role, "NguoiDung", user.AvatarUrl);
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
                await SignInUserAsync(patient.MaBenhNhan.ToString(), patient.HoTen, "BenhNhan", "BenhNhan", patient.AvatarUrl);
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
        // Challenge: khi thành công, provider sẽ sign-in vào scheme DefaultSignInScheme ("ExternalCookie")
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Lỗi từ nhà cung cấp: {remoteError}";
            return RedirectToAction("Login");
        }

        // Đọc thông tin từ external cookie mà provider đã ghi vào sau khi OAuth thành công
        // ASP.NET Core OAuth middleware dùng DefaultSignInScheme = "ExternalCookie"
        AuthenticateResult? info = null;
        foreach (var scheme in new[] { "ExternalCookie", "Identity.External", CookieAuthenticationDefaults.AuthenticationScheme })
        {
            try
            {
                info = await HttpContext.AuthenticateAsync(scheme);
                if (info?.Succeeded == true) break;
            }
            catch { /* scheme không tồn tại, thử cái tiếp theo */ }
        }

        if (info == null || !info.Succeeded)
        {
            TempData["ErrorMessage"] = "Không thể xác thực với nhà cung cấp bên ngoài. Vui lòng thử lại.";
            return RedirectToAction("Login");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name  = info.Principal.FindFirstValue(ClaimTypes.Name)
                    ?? info.Principal.FindFirstValue("urn:github:name")
                    ?? info.Principal.FindFirstValue("name");

        // Xóa đúng cookie external tạm thời (không phải cookie đăng nhập chính)
        try { await HttpContext.SignOutAsync("ExternalCookie"); } catch { }
        try { await HttpContext.SignOutAsync("Identity.External"); } catch { }

        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Không thể lấy email từ nhà cung cấp. Vui lòng đăng ký thủ công.";
            return RedirectToAction("Login");
        }

        // 1. Kiểm tra bảng NguoiDung (staff/admin)
        var users = await _nguoiDungRepository.FindAsync(u => u.Email == email);
        var user = users.FirstOrDefault();
        if (user != null)
        {
            await SignInUserAsync(user.MaNguoiDung.ToString(), user.HoTen, user.Role, "NguoiDung", user.AvatarUrl);
            return RedirectToLocal(returnUrl);
        }

        // 2. Kiểm tra bảng BenhNhan
        var patients = await _benhNhanRepository.FindAsync(p => p.Email == email);
        var patient = patients.FirstOrDefault();
        if (patient != null)
        {
            await SignInUserAsync(patient.MaBenhNhan.ToString(), patient.HoTen, "BenhNhan", "BenhNhan", patient.AvatarUrl);
            return RedirectToLocal(returnUrl);
        }

        // 3. Tạo tài khoản mới và tự động đăng nhập (luồng OAuth2 tiêu chuẩn)
        var newPatient = new BenhNhan
        {
            HoTen = string.IsNullOrWhiteSpace(name) ? "Người dùng mới" : name,
            Email = email,
            GioiTinh = "Khac",
            NgaySinh = new DateTime(2000, 1, 1),
            MatKhau = Guid.NewGuid().ToString()[..8],
            NgayTao = DateTime.Now
        };
        await _benhNhanRepository.AddAsync(newPatient);

        await SignInUserAsync(newPatient.MaBenhNhan.ToString(), newPatient.HoTen, "BenhNhan", "BenhNhan", newPatient.AvatarUrl);
        TempData["SuccessMessage"] = $"Xin chào {newPatient.HoTen}! Tài khoản đã được tạo tự động từ tài khoản {email}.";
        return RedirectToAction("Index", "Patient");
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

        if (model.NgaySinh.Value.Year < 1900)
        {
            ModelState.AddModelError("NgaySinh", "Năm sinh không hợp lệ (phải từ năm 1900 trở đi).");
            return View(model);
        }

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
            NgaySinh = model.NgaySinh.Value,
            GioiTinh = model.GioiTinh,
            SoDienThoai = model.SoDienThoai,
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            MatKhau = model.MatKhau, // Trong thực tế nên Hash mật khẩu
            CCCD = string.IsNullOrWhiteSpace(model.CCCD) ? null : model.CCCD.Trim(),
            NgayTao = DateTime.Now
        };

        await _benhNhanRepository.AddAsync(benhNhan);
        // KHÔNG tự động đăng nhập — redirect về trang Login để người dùng tự đăng nhập
        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập bằng số điện thoại hoặc email của bạn.";
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

    private async Task SignInUserAsync(string userId, string name, string role, string userType, string? avatarUrl = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserType", userType)
        };

        if (!string.IsNullOrEmpty(avatarUrl))
        {
            claims.Add(new Claim("AvatarUrl", avatarUrl));
        }

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

    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [Authorize]
    public IActionResult Settings()
    {
        return View();
    }
}
