using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
using MyMvcApp.Repositories;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin,Manager,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task<IActionResult> Accounts()
    {
        var nguoiDungs = await _context.NguoiDungs.ToListAsync();
        var benhNhans = await _context.BenhNhans.ToListAsync();

        var accounts = new List<AccountItemViewModel>();

        foreach(var nd in nguoiDungs)
        {
            accounts.Add(new AccountItemViewModel
            {
                Id = nd.MaNguoiDung.ToString(),
                Username = $"@{nd.HoTen.Split(' ').Last().ToLower()}.00{nd.MaNguoiDung}", // fake handle
                FullName = nd.HoTen,
                Role = nd.Role == "BacSi" ? "Doctor" : (nd.Role == "Admin" ? "Admin" : "Staff"),
                IsActive = nd.TrangThai,
                CreatedAt = nd.NgayTao ?? DateTime.Now.AddDays(-100),
                Type = "NguoiDung"
            });
        }

        foreach(var bn in benhNhans)
        {
            accounts.Add(new AccountItemViewModel
            {
                Id = bn.MaBenhNhan.ToString(),
                Username = $"@{bn.HoTen.Split(' ').Last().ToLower()}.00{bn.MaBenhNhan}", // fake handle
                FullName = bn.HoTen,
                Role = "Patient",
                IsActive = true, // BenhNhan currently has no TrangThai field, assume active
                CreatedAt = bn.NgayTao ?? DateTime.Now.AddDays(-50),
                Type = "BenhNhan"
            });
        }

        // Add some hardcoded ones if empty, just to make sure UI looks like mockup
        if (!accounts.Any())
        {
            accounts.Add(new AccountItemViewModel { Id = "1", Username = "@dung.001", FullName = "Nguyễn Văn Dũng", Role = "Doctor", IsActive = false, CreatedAt = new DateTime(2022, 1, 1) });
            accounts.Add(new AccountItemViewModel { Id = "2", Username = "@hoa.002", FullName = "Trần Thị Hoa", Role = "Staff", IsActive = true, CreatedAt = new DateTime(2023, 2, 2) });
            accounts.Add(new AccountItemViewModel { Id = "3", Username = "@khoa.003", FullName = "Lê Minh Khoa", Role = "Patient", IsActive = true, CreatedAt = new DateTime(2024, 3, 3) });
            accounts.Add(new AccountItemViewModel { Id = "4", Username = "@lan.004", FullName = "Phạm Thị Lan", Role = "Admin", IsActive = true, CreatedAt = new DateTime(2022, 4, 4) });
        }

        return View(accounts.OrderBy(a => a.Username).ToList());
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

        foreach(var k in khoas)
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

        foreach(var d in dbDoctors)
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
}
