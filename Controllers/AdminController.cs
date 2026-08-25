using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
using MyMvcApp.Repositories;
using MyMvcApp.Services;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin,Manager,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public AdminController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index()
    {
        // Calculate fake metrics based on db + fake math for the UI mockup
        var nguoiDungs = await _context.NguoiDungs.ToListAsync();
        var benhNhans = await _context.BenhNhans.ToListAsync();

        var totalAccounts = nguoiDungs.Count() + benhNhans.Count() + 2000; // Fake big number for the mockup
        var doctors = nguoiDungs.Count(n => n.Role == "BacSi") + 40;
        var staff = nguoiDungs.Count(n => n.Role == "LeTan" || n.Role == "Manager") + 130;
        var patients = benhNhans.Count() + 1840;

        var activeAccounts = nguoiDungs.Count(n => n.TrangThai) + 1900;
        var lockedAccounts = nguoiDungs.Count(n => !n.TrangThai) + 20;

        var model = new AdminDashboardViewModel
        {
            TotalAccounts = totalAccounts,
            TotalDoctors = doctors,
            TotalStaff = staff,
            TotalPatients = patients,
            ActiveAccounts = activeAccounts,
            LockedAccounts = lockedAccounts
        };

        return View(model);
    }

    public async Task<IActionResult> Accounts(string searchString, string roleFilter, string sortOrder)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentRole"] = roleFilter;
        ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["RoleSortParm"] = sortOrder == "Role" ? "role_desc" : "Role";
        ViewData["CurrentSort"] = sortOrder;

        var nguoiDungs = await _context.NguoiDungs.ToListAsync();
        var benhNhans = await _context.BenhNhans.ToListAsync();

        var accounts = new List<AccountItemViewModel>();

        foreach (var nd in nguoiDungs)
        {
            accounts.Add(new AccountItemViewModel
            {
                Id = nd.MaNguoiDung.ToString(),
                Username = $"@{nd.HoTen.Split(' ').Last().ToLower()}.00{nd.MaNguoiDung}", // fake handle
                FullName = nd.HoTen,
                Role = nd.Role == "BacSi" ? "Doctor" : (nd.Role == "Admin" ? "Admin" : (nd.Role == "Manager" ? "Manager" : "Staff")),
                IsActive = nd.TrangThai,
                CreatedAt = nd.NgayTao ?? DateTime.Now.AddDays(-100),
                Type = "NguoiDung",
                AvatarUrl = nd.AvatarUrl
            });
        }

        foreach (var bn in benhNhans)
        {
            accounts.Add(new AccountItemViewModel
            {
                Id = bn.MaBenhNhan.ToString(),
                Username = $"@{bn.HoTen.Split(' ').Last().ToLower()}.00{bn.MaBenhNhan}", // fake handle
                FullName = bn.HoTen,
                Role = "Patient",
                IsActive = true, // BenhNhan currently has no TrangThai field, assume active
                CreatedAt = bn.NgayTao ?? DateTime.Now.AddDays(-50),
                Type = "BenhNhan",
                AvatarUrl = bn.AvatarUrl
            });
        }

        // Apply filters
        if (!String.IsNullOrEmpty(searchString))
        {
            accounts = accounts.Where(s => s.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || s.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!String.IsNullOrEmpty(roleFilter))
        {
            accounts = accounts.Where(s => s.Role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply sorting
        switch (sortOrder)
        {
            case "name_desc":
                accounts = accounts.OrderByDescending(s => s.FullName).ToList();
                break;
            case "Date":
                accounts = accounts.OrderBy(s => s.CreatedAt).ToList();
                break;
            case "date_desc":
                accounts = accounts.OrderByDescending(s => s.CreatedAt).ToList();
                break;
            case "Role":
                accounts = accounts.OrderBy(s => s.Role).ToList();
                break;
            case "role_desc":
                accounts = accounts.OrderByDescending(s => s.Role).ToList();
                break;
            default:
                accounts = accounts.OrderBy(s => s.FullName).ToList();
                break;
        }

        return View(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(NguoiDung model)
    {
        model.NgayTao = DateTime.Now;
        model.TrangThai = true;
        // In real app, hash password here: model.MatKhau = HashPassword(model.MatKhau);
        await _context.NguoiDungs.AddAsync(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Thêm tài khoản thành công!";
        return RedirectToAction("Accounts");
    }

    [HttpPost]
    public async Task<IActionResult> EditAccount(string Id, string Type, string HoTen, string Role, IFormFile? AvatarFile)
    {
        string? uploadedUrl = null;
        if (AvatarFile != null)
        {
            try
            {
                uploadedUrl = await _imageService.UploadImageAsync(AvatarFile);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi upload ảnh: {ex.Message}";
                return RedirectToAction("Accounts");
            }
        }

        if (Type == "NguoiDung" && int.TryParse(Id, out int ndId))
        {
            var user = await _context.NguoiDungs.FindAsync(ndId);
            if (user != null)
            {
                user.HoTen = HoTen;
                if (uploadedUrl != null)
                    user.AvatarUrl = uploadedUrl;
                if (!string.IsNullOrEmpty(Role))
                    user.Role = Role;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật tài khoản nhân sự thành công!";
            }
        }
        else if (Type == "BenhNhan" && int.TryParse(Id, out int bnId))
        {
            var patient = await _context.BenhNhans.FindAsync(bnId);
            if (patient != null)
            {
                patient.HoTen = HoTen;
                if (uploadedUrl != null)
                    patient.AvatarUrl = uploadedUrl;
                // Bệnh nhân không đổi role
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật tài khoản bệnh nhân thành công!";
            }
        }
        return RedirectToAction("Accounts");
    }

    public async Task<IActionResult> ToggleLockAccount(string id, string type)
    {
        if (type == "NguoiDung" && int.TryParse(id, out int ndId))
        {
            var user = await _context.NguoiDungs.FindAsync(ndId);
            if (user != null)
            {
                user.TrangThai = !user.TrangThai;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = user.TrangThai ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
            }
        }
        // Bệnh nhân chưa có cột TrangThai, tạm thời bỏ qua
        return RedirectToAction("Accounts");
    }

    public async Task<IActionResult> DeleteAccount(string id, string type)
    {
        if (type == "NguoiDung" && int.TryParse(id, out int ndId))
        {
            var user = await _context.NguoiDungs.FindAsync(ndId);
            if (user != null)
            {
                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản nhân viên.";
            }
        }
        else if (type == "BenhNhan" && int.TryParse(id, out int bnId))
        {
            var patient = await _context.BenhNhans.FindAsync(bnId);
            if (patient != null)
            {
                _context.BenhNhans.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản bệnh nhân.";
            }
        }
        return RedirectToAction("Accounts");
    }

    public async Task<IActionResult> ExportAccounts()
    {
        var nguoiDungs = await _context.NguoiDungs.ToListAsync();
        var benhNhans = await _context.BenhNhans.ToListAsync();

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("ID,Type,FullName,Role,Email,Phone,IsActive,CreatedAt");

        foreach (var nd in nguoiDungs)
        {
            builder.AppendLine($"{nd.MaNguoiDung},NguoiDung,{nd.HoTen},{nd.Role},{nd.Email},{nd.SoDienThoai},{nd.TrangThai},{nd.NgayTao}");
        }

        foreach (var bn in benhNhans)
        {
            builder.AppendLine($"{bn.MaBenhNhan},BenhNhan,{bn.HoTen},Patient,{bn.Email},{bn.SoDienThoai},True,{bn.NgayTao}");
        }

        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "accounts_export.csv");
    }

    [HttpPost]
    public async Task<IActionResult> ImportAccounts(IFormFile importFile)
    {
        if (importFile == null || importFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file CSV để tải lên.";
            return RedirectToAction("Accounts");
        }

        using (var stream = new StreamReader(importFile.OpenReadStream()))
        {
            var headerLine = await stream.ReadLineAsync();
            int importCount = 0;
            while (!stream.EndOfStream)
            {
                var line = await stream.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length >= 4)
                {
                    // Giả định CSV có cấu trúc: HoTen, Email, SoDienThoai, Role
                    var newUser = new NguoiDung
                    {
                        HoTen = values[0].Trim(),
                        Email = values[1].Trim(),
                        SoDienThoai = values[2].Trim(),
                        Role = values[3].Trim(),
                        MatKhau = Guid.NewGuid().ToString().Substring(0, 8),
                        TrangThai = true,
                        NgayTao = DateTime.Now
                    };
                    await _context.NguoiDungs.AddAsync(newUser);
                    importCount++;
                }
            }
            if (importCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã import thành công {importCount} tài khoản nhân sự.";
            }
        }
        return RedirectToAction("Accounts");
    }

    public async Task<IActionResult> Doctors()
    {
        var doctorsQuery = _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => n.Role == "BacSi");

        var dbDoctors = await doctorsQuery.ToListAsync();
        var khoas = await _context.ChuyenKhoas.ToListAsync();

        var model = new DoctorListViewModel();

        var colorClasses = new[] { "badge-role-Staff", "badge-role-Doctor", "badge-status-Locked", "badge-role-Patient", "badge-status-Active", "badge-role-Admin" };
        int colorIdx = 0;

        foreach (var k in khoas)
        {
            model.Specialities.Add(new KhoaSummary
            {
                KhoaName = k.TenChuyenKhoa,
                Count = dbDoctors.Count(d => d.MaChuyenKhoa == k.MaChuyenKhoa),
                ColorClass = colorClasses[colorIdx % colorClasses.Length]
            });
            colorIdx++;
        }

        // Add mock if db is empty
        if (!model.Specialities.Any())
        {
            model.Specialities.Add(new KhoaSummary { KhoaName = "Khoa Nội", Count = 20, ColorClass = "badge-role-Staff" });
            model.Specialities.Add(new KhoaSummary { KhoaName = "Khoa Ngoại", Count = 20, ColorClass = "badge-status-Active" });
            model.Specialities.Add(new KhoaSummary { KhoaName = "Nhi khoa", Count = 20, ColorClass = "badge-role-Admin" });
            model.Specialities.Add(new KhoaSummary { KhoaName = "Sản phụ khoa", Count = 20, ColorClass = "badge-role-Doctor" });
            model.Specialities.Add(new KhoaSummary { KhoaName = "Nha khoa", Count = 20, ColorClass = "badge-role-Staff" });
            model.Specialities.Add(new KhoaSummary { KhoaName = "Da liễu", Count = 20, ColorClass = "badge-status-Locked" });
        }

        foreach (var d in dbDoctors)
        {
            model.Doctors.Add(new DoctorItemViewModel
            {
                Id = d.MaNguoiDung,
                FullName = d.HoTen,
                Title = "BS", // In a real system, you might store this in a separate field
                Speciality = d.ChuyenKhoa?.TenChuyenKhoa ?? "Đa khoa",
                ClinicRoom = $"Phòng {100 + d.MaNguoiDung} - Tầng {d.MaNguoiDung % 3 + 1}",
                WorkSchedule = "Thứ 2, 4, 6 - Sáng",
                IsActive = d.TrangThai
            });
        }

        if (!model.Doctors.Any())
        {
            model.Doctors.Add(new DoctorItemViewModel { Id = 1, FullName = "Nguyễn Minh Khoa", Title = "BS.", Speciality = "Khoa Nội", ClinicRoom = "Phòng 101 - Tầng 1", WorkSchedule = "Thứ 2, 4, 6 - Sáng", IsActive = true });
            model.Doctors.Add(new DoctorItemViewModel { Id = 2, FullName = "Trần Hữu Lan", Title = "ThS.BS.", Speciality = "Khoa Ngoại", ClinicRoom = "Phòng 204 - Tầng 2", WorkSchedule = "Thứ 3, 5, 7 - Chiều", IsActive = true });
            model.Doctors.Add(new DoctorItemViewModel { Id = 3, FullName = "Lê Quốc Minh", Title = "BS.CKII.", Speciality = "Nhi khoa", ClinicRoom = "Phòng 302 - Tầng 3", WorkSchedule = "Thứ 2, 3, 4 - Cả ngày", IsActive = true });
        }

        model.TotalDoctors = model.Doctors.Count;

        return View(model);
    }

    public IActionResult Roles()
    {
        return View("ComingSoon");
    }

    public IActionResult Logs()
    {
        return View("ComingSoon");
    }

    // ============ CHUYÊN KHOA CRUD ============
    public async Task<IActionResult> Specialties()
    {
        var list = await _context.ChuyenKhoas
            .Include(c => c.NguoiDungs)
            .ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpecialty(string TenChuyenKhoa, string MoTa)
    {
        if (!string.IsNullOrWhiteSpace(TenChuyenKhoa))
        {
            _context.ChuyenKhoas.Add(new ChuyenKhoa { TenChuyenKhoa = TenChuyenKhoa, MoTa = MoTa });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm chuyên khoa thành công!";
        }
        return RedirectToAction(nameof(Specialties));
    }

    [HttpPost]
    public async Task<IActionResult> EditSpecialty(int id, string TenChuyenKhoa, string MoTa)
    {
        var ck = await _context.ChuyenKhoas.FindAsync(id);
        if (ck != null)
        {
            ck.TenChuyenKhoa = TenChuyenKhoa;
            ck.MoTa = MoTa;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật chuyên khoa thành công!";
        }
        return RedirectToAction(nameof(Specialties));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSpecialty(int id)
    {
        var ck = await _context.ChuyenKhoas.Include(c => c.NguoiDungs).FirstOrDefaultAsync(c => c.MaChuyenKhoa == id);
        if (ck != null && !ck.NguoiDungs.Any())
        {
            _context.ChuyenKhoas.Remove(ck);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa chuyên khoa thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể xóa chuyên khoa đang có bác sĩ!";
        }
        return RedirectToAction(nameof(Specialties));
    }

    // ============ DỊCH VỤ CRUD ============
    public async Task<IActionResult> Services()
    {
        var list = await _context.DichVus.ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateService(string TenDichVu, decimal DonGia, string MoTa)
    {
        if (!string.IsNullOrWhiteSpace(TenDichVu))
        {
            _context.DichVus.Add(new DichVu { TenDichVu = TenDichVu, DonGia = DonGia, MoTa = MoTa });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm dịch vụ thành công!";
        }
        return RedirectToAction(nameof(Services));
    }

    [HttpPost]
    public async Task<IActionResult> EditService(int id, string TenDichVu, decimal DonGia, string MoTa)
    {
        var dv = await _context.DichVus.FindAsync(id);
        if (dv != null)
        {
            dv.TenDichVu = TenDichVu;
            dv.DonGia = DonGia;
            dv.MoTa = MoTa;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
        }
        return RedirectToAction(nameof(Services));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteService(int id)
    {
        var dv = await _context.DichVus.Include(d => d.ChiTietDichVus).FirstOrDefaultAsync(d => d.MaDichVu == id);
        if (dv != null && !dv.ChiTietDichVus.Any())
        {
            _context.DichVus.Remove(dv);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa dịch vụ thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể xóa dịch vụ đang được sử dụng trong phiếu khám!";
        }
        return RedirectToAction(nameof(Services));
    }
}

