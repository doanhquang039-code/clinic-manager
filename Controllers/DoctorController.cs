using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
using MyMvcApp.Services;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "BacSi")]
public class DoctorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public DoctorController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    private int GetCurrentDoctorId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out int docId))
        {
            return docId;
        }
        return 0;
    }

    // ===== DASHBOARD =====
    public async Task<IActionResult> Index()
    {
        var docId = GetCurrentDoctorId();
        
        var todayAppointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId && l.NgayKham.Date == DateTime.Today)
            .OrderBy(l => l.GioKham)
            .ToListAsync();

        ViewBag.PendingAppointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId && l.TrangThai == "ChoXacNhan" && l.NgayKham.Date >= DateTime.Today)
            .OrderBy(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();

        ViewBag.TotalPatients = await _context.LichKhams
            .Where(l => l.MaBacSi == docId)
            .Select(l => l.MaBenhNhan)
            .Distinct()
            .CountAsync();

        ViewBag.TotalConsultations = await _context.PhieuKhams
            .Where(p => p.MaBacSi == docId)
            .CountAsync();

        return View(todayAppointments);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveAppointment(int id, string action)
    {
        var docId = GetCurrentDoctorId();
        var appointment = await _context.LichKhams
            .FirstOrDefaultAsync(l => l.MaLichKham == id && l.MaBacSi == docId);
            
        if (appointment != null)
        {
            appointment.TrangThai = action == "approve" ? "DaXacNhan" : "DaHuy";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = action == "approve"
                ? $"Đã xác nhận lịch khám #{id}."
                : $"Đã hủy lịch khám #{id}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ===== CONSULTATION (Khám bệnh) =====
    public async Task<IActionResult> Consultation(int id)
    {
        var docId = GetCurrentDoctorId();
        
        var appointment = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .FirstOrDefaultAsync(l => l.MaLichKham == id && l.MaBacSi == docId);

        if (appointment == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy lịch khám hoặc bạn không có quyền truy cập.";
            return RedirectToAction(nameof(Index));
        }

        // Lấy lịch sử khám của bệnh nhân này (trừ lần khám hiện tại)
        var patientHistory = await _context.PhieuKhams
            .Include(p => p.ChiTietDonThuocs).ThenInclude(c => c.Thuoc)
            .Include(p => p.LichKham)
            .Where(p => p.MaBenhNhan == appointment.MaBenhNhan)
            .OrderByDescending(p => p.NgayKham)
            .Take(10)
            .ToListAsync();

        ViewBag.Appointment = appointment;
        ViewBag.PatientHistory = patientHistory;

        // Lấy danh sách thuốc còn hàng
        ViewBag.Medicines = await _context.Thuocs
            .Where(t => t.TrangThai == "ConHang" && t.SoLuongTon > 0)
            .OrderBy(t => t.TenThuoc)
            .ToListAsync();

        var phieuKham = new PhieuKham
        {
            MaLichKham = appointment.MaLichKham,
            MaBenhNhan = appointment.MaBenhNhan,
            MaBacSi = appointment.MaBacSi,
            TrieuChung = appointment.LyDoKham,
            NgayKham = DateTime.Now
        };

        return View(phieuKham);
    }

    [HttpPost]
    public async Task<IActionResult> SaveConsultation(PhieuKham model, List<int> ThuocIds, List<int> SoLuongs, List<string> LieuDungs)
    {
        ModelState.Remove("BenhNhan");
        ModelState.Remove("BacSi");
        ModelState.Remove("LichKham");
        ModelState.Remove("HoaDon");

        if (ModelState.IsValid)
        {
            var appointment = await _context.LichKhams.FindAsync(model.MaLichKham);
            if (appointment != null)
            {
                appointment.TrangThai = "DaKham";
                
                model.NgayKham = DateTime.Now;
                await _context.PhieuKhams.AddAsync(model);
                await _context.SaveChangesAsync();

                // Lưu đơn thuốc nếu có
                if (ThuocIds != null && ThuocIds.Count > 0)
                {
                    for (int i = 0; i < ThuocIds.Count; i++)
                    {
                        if (ThuocIds[i] <= 0) continue;

                        var thuoc = await _context.Thuocs.FindAsync(ThuocIds[i]);
                        if (thuoc == null) continue;

                        var soLuong = (i < SoLuongs.Count && SoLuongs[i] > 0) ? SoLuongs[i] : 1;

                        var chiTiet = new ChiTietDonThuoc
                        {
                            MaPhieuKham = model.MaPhieuKham,
                            MaThuoc = ThuocIds[i],
                            SoLuong = soLuong,
                            DonGia = thuoc.DonGia,
                            LieuDung = (i < LieuDungs.Count) ? LieuDungs[i] : "",
                        };
                        _context.ChiTietDonThuocs.Add(chiTiet);

                        // Trừ tồn kho
                        thuoc.SoLuongTon = Math.Max(0, thuoc.SoLuongTon - soLuong);
                    }
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Lưu kết quả khám bệnh và đơn thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Chẩn đoán.";
        return RedirectToAction("Consultation", new { id = model.MaLichKham });
    }

    // Ajax: Tìm kiếm thuốc
    [HttpGet]
    public async Task<IActionResult> SearchMedicines(string q)
    {
        var medicines = await _context.Thuocs
            .Where(t => t.TrangThai == "ConHang" && t.SoLuongTon > 0 
                        && (string.IsNullOrEmpty(q) || t.TenThuoc.Contains(q)))
            .OrderBy(t => t.TenThuoc)
            .Take(20)
            .Select(t => new { t.MaThuoc, t.TenThuoc, t.DonViTinh, t.DonGia, t.SoLuongTon })
            .ToListAsync();

        return Json(medicines);
    }

    // ===== HISTORY =====
    public async Task<IActionResult> History()
    {
        var docId = GetCurrentDoctorId();
        
        var history = await _context.PhieuKhams
            .Include(p => p.BenhNhan)
            .Include(p => p.ChiTietDonThuocs).ThenInclude(c => c.Thuoc)
            .Where(p => p.MaBacSi == docId)
            .OrderByDescending(p => p.NgayKham)
            .Take(50)
            .ToListAsync();

        return View(history);
    }

    // ===== APPOINTMENTS =====
    public async Task<IActionResult> Appointments(string status = "all")
    {
        var docId = GetCurrentDoctorId();
        ViewBag.CurrentStatus = status;

        var query = _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId)
            .AsQueryable();

        if (status == "pending")
            query = query.Where(l => l.TrangThai == "ChoXacNhan");
        else if (status == "approved")
            query = query.Where(l => l.TrangThai == "DaXacNhan");
        else if (status == "done")
            query = query.Where(l => l.TrangThai == "DaKham");
        
        var list = await query
            .OrderByDescending(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();

        return View(list);
    }

    // ===== MONTHLY SCHEDULE =====
    public async Task<IActionResult> MonthlySchedule(int? month, int? year)
    {
        var docId = GetCurrentDoctorId();
        
        var currentYear = year ?? DateTime.Today.Year;
        var currentMonth = month ?? DateTime.Today.Month;
        
        if (currentMonth < 1) { currentMonth = 12; currentYear--; }
        if (currentMonth > 12) { currentMonth = 1; currentYear++; }

        var startDate = new DateTime(currentYear, currentMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        ViewBag.CurrentMonth = currentMonth;
        ViewBag.CurrentYear = currentYear;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;

        var appointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId
                        && l.NgayKham.Date >= startDate.Date
                        && l.NgayKham.Date <= endDate.Date)
            .OrderBy(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();

        return View("MonthlySchedule", appointments);
    }

    // ===== PROFILE =====
    public async Task<IActionResult> Profile()
    {
        var docId = GetCurrentDoctorId();
        var doctor = await _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .FirstOrDefaultAsync(n => n.MaNguoiDung == docId);

        if (doctor == null) return NotFound();
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(NguoiDung model, IFormFile? AvatarFile)
    {
        var docId = GetCurrentDoctorId();
        var doctor = await _context.NguoiDungs.FindAsync(docId);

        if (doctor == null) return NotFound();

        doctor.HoTen = model.HoTen;
        doctor.SoDienThoai = model.SoDienThoai;
        doctor.Email = model.Email;

        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var avatarUrl = await _imageService.UploadImageAsync(AvatarFile);
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                doctor.AvatarUrl = avatarUrl;
            }
        }

        await _context.SaveChangesAsync();

        // Refresh avatar claim in cookie
        var claims = User.Claims.ToList();
        var avatarClaim = claims.FirstOrDefault(c => c.Type == "AvatarUrl");
        if (avatarClaim != null) claims.Remove(avatarClaim);
        if (!string.IsNullOrEmpty(doctor.AvatarUrl))
            claims.Add(new System.Security.Claims.Claim("AvatarUrl", doctor.AvatarUrl));

        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
    {
        var docId = GetCurrentDoctorId();
        var doctor = await _context.NguoiDungs.FindAsync(docId);

        if (doctor == null) return NotFound();

        if (doctor.MatKhau != CurrentPassword)
        {
            TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
            return RedirectToAction(nameof(Profile));
        }

        if (NewPassword != ConfirmPassword)
        {
            TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp!";
            return RedirectToAction(nameof(Profile));
        }

        doctor.MatKhau = NewPassword;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Profile));
    }
}
