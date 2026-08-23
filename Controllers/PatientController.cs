using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "BenhNhan")]
public class PatientController : Controller
{
    private readonly ApplicationDbContext _context;

    public PatientController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentPatientId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out int patientId))
        {
            return patientId;
        }
        return 0;
    }

    public async Task<IActionResult> Index()
    {
        var patientId = GetCurrentPatientId();
        
        var upcomingAppointments = await _context.LichKhams
            .Include(l => l.BacSi)
            .ThenInclude(b => b.ChuyenKhoa)
            .Where(l => l.MaBenhNhan == patientId && l.NgayKham >= DateTime.Today)
            .OrderBy(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .Take(5)
            .ToListAsync();

        return View(upcomingAppointments);
    }

    public async Task<IActionResult> BookAppointment()
    {
        ViewBag.Specialties = await _context.ChuyenKhoas.ToListAsync();
        
        // Select the full NguoiDung entity or create a typed DTO to avoid JSON serialization issues with anonymous types
        var docs = await _context.NguoiDungs
            .Where(n => n.Role == "BacSi" && n.TrangThai)
            .Select(d => new 
            { 
                MaNguoiDung = d.MaNguoiDung, 
                HoTen = d.HoTen, 
                MaChuyenKhoa = d.MaChuyenKhoa 
            })
            .ToListAsync();
            
        // Convert to a list of dicts to ensure 100% safe JSON serialization
        ViewBag.Doctors = docs.Select(d => new Dictionary<string, object>
        {
            { "maNguoiDung", d.MaNguoiDung },
            { "hoTen", d.HoTen },
            { "maChuyenKhoa", d.MaChuyenKhoa ?? 0 }
        }).ToList();
            
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(LichKham model)
    {
        var patientId = GetCurrentPatientId();
        model.MaBenhNhan = patientId;
        model.TrangThai = "ChoXacNhan";
        model.NgayTao = DateTime.Now;

        // Xóa ModelState các khóa ngoại vì chúng ta gán tay hoặc không cần validate form
        ModelState.Remove("BenhNhan");
        ModelState.Remove("BacSi");
        ModelState.Remove("PhieuKham");

        if (ModelState.IsValid)
        {
            // Kiểm tra trùng lịch của bác sĩ
            var exists = await _context.LichKhams.AnyAsync(l => 
                l.MaBacSi == model.MaBacSi && 
                l.NgayKham.Date == model.NgayKham.Date && 
                l.GioKham == model.GioKham);

            if (exists)
            {
                TempData["ErrorMessage"] = "Bác sĩ đã có lịch hẹn vào thời gian này. Vui lòng chọn giờ khác.";
                return RedirectToAction(nameof(BookAppointment));
            }

            // Kiểm tra lịch hẹn trong quá khứ
            var appointmentDateTime = model.NgayKham.Date.Add(model.GioKham);
            if (appointmentDateTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Không thể đặt lịch khám trong quá khứ.";
                return RedirectToAction(nameof(BookAppointment));
            }

            await _context.LichKhams.AddAsync(model);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Đặt lịch khám thành công! Vui lòng chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
        return RedirectToAction(nameof(BookAppointment));
    }

    public async Task<IActionResult> MedicalRecords()
    {
        var patientId = GetCurrentPatientId();
        
        var records = await _context.PhieuKhams
            .Include(p => p.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .Include(p => p.LichKham)
            .Where(p => p.MaBenhNhan == patientId)
            .OrderByDescending(p => p.NgayKham)
            .ToListAsync();

        return View(records);
    }

    public async Task<IActionResult> RecordDetail(int id)
    {
        var patientId = GetCurrentPatientId();
        var record = await _context.PhieuKhams
            .Include(p => p.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .Include(p => p.BenhNhan)
            .Include(p => p.LichKham)
            .Include(p => p.HoaDon)
            .FirstOrDefaultAsync(p => p.MaPhieuKham == id && p.MaBenhNhan == patientId);

        if (record == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy phiếu khám.";
            return RedirectToAction(nameof(MedicalRecords));
        }

        return View(record);
    }

    [HttpPost]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var patientId = GetCurrentPatientId();
        var appointment = await _context.LichKhams
            .FirstOrDefaultAsync(l => l.MaLichKham == id && l.MaBenhNhan == patientId);

        if (appointment == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy lịch khám.";
            return RedirectToAction(nameof(Index));
        }

        if (appointment.TrangThai != "ChoXacNhan")
        {
            TempData["ErrorMessage"] = "Chỉ có thể hủy lịch khám đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        appointment.TrangThai = "DaHuy";
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã hủy lịch khám thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Profile()
    {
        var patientId = GetCurrentPatientId();
        var patient = await _context.BenhNhans.FindAsync(patientId);
        if (patient == null) return RedirectToAction(nameof(Index));
        return View(patient);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(BenhNhan model)
    {
        var patientId = GetCurrentPatientId();
        var patient = await _context.BenhNhans.FindAsync(patientId);
        if (patient == null) return RedirectToAction(nameof(Index));

        // Chỉ cập nhật các trường cho phép
        patient.SoDienThoai = model.SoDienThoai;
        patient.DiaChi = model.DiaChi;
        patient.DiUng = model.DiUng;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
        return RedirectToAction(nameof(Profile));
    }

    // ============ ĐÁNH GIÁ BÁC SĨ ============
    [HttpPost]
    public async Task<IActionResult> RateDoctor(int phieuKhamId, int rating, string review)
    {
        var patientId = GetCurrentPatientId();
        var record = await _context.PhieuKhams
            .FirstOrDefaultAsync(p => p.MaPhieuKham == phieuKhamId && p.MaBenhNhan == patientId);

        if (record != null && rating >= 1 && rating <= 5)
        {
            record.DanhGia = rating;
            record.NhanXet = review;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Cảm ơn bạn đã đánh giá {rating}★! Phản hồi của bạn giúp chúng tôi cải thiện chất lượng dịch vụ.";
        }

        return RedirectToAction(nameof(MedicalRecords));
    }
    // ============ XEM THUỐC ============
    public async Task<IActionResult> Drugs()
    {
        var drugs = await _context.Thuocs
            .OrderBy(t => t.TenThuoc)
            .ToListAsync();
        return View(drugs);
    }

    // ============ CÀI ĐẶT ============
    public IActionResult Settings()
    {
        // For demonstration purposes, we will pass a dummy settings object.
        // Real app would load this from DB/Preferences.
        ViewBag.SmsNotification = true;
        ViewBag.EmailNotification = true;
        return View();
    }

    // ============ ĐỘI NGŨ BÁC SĨ ============
    public async Task<IActionResult> Doctors()
    {
        var doctors = await _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => n.Role == "BacSi" && n.TrangThai == true)
            .OrderBy(n => n.HoTen)
            .ToListAsync();
        return View(doctors);
    }

    // ============ CHUYÊN KHOA ============
    public async Task<IActionResult> Specialties()
    {
        var specialties = await _context.ChuyenKhoas
            .OrderBy(c => c.TenChuyenKhoa)
            .ToListAsync();
        return View(specialties);
    }

    // ============ HÓA ĐƠN VÀ ĐƠN THUỐC ============
    public async Task<IActionResult> Invoices()
    {
        var patientId = GetCurrentPatientId();
        var invoices = await _context.HoaDons
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p.LichKham)
            .Include(h => h.NguoiLap)
            .Where(h => h.PhieuKham != null && h.PhieuKham.MaBenhNhan == patientId)
            .OrderByDescending(h => h.NgayLap)
            .ToListAsync();
            
        return View(invoices);
    }

    public async Task<IActionResult> InvoiceDetail(int id)
    {
        var patientId = GetCurrentPatientId();
        var invoice = await _context.HoaDons
            .Include(h => h.NguoiLap)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p.BacSi)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p.ChiTietDonThuocs)
                .ThenInclude(c => c.Thuoc)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p.ChiTietDichVus)
                .ThenInclude(c => c.DichVu)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id && h.PhieuKham != null && h.PhieuKham.MaBenhNhan == patientId);

        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
            return RedirectToAction(nameof(Invoices));
        }

        return View(invoice);
    }
}
