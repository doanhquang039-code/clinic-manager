using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "BacSi")]
public class DoctorController : Controller
{
    private readonly ApplicationDbContext _context;

    public DoctorController(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task<IActionResult> Index()
    {
        var docId = GetCurrentDoctorId();
        
        // Lấy lịch khám hôm nay của bác sĩ này
        var todayAppointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId && l.NgayKham.Date == DateTime.Today)
            .OrderBy(l => l.GioKham)
            .ToListAsync();

        // Lấy các lịch khám chờ xác nhận (từ hôm nay trở đi)
        ViewBag.PendingAppointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId && l.TrangThai == "ChoXacNhan" && l.NgayKham.Date >= DateTime.Today)
            .OrderBy(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();

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

        // Tạo ViewModel hoặc trực tiếp tạo PhieuKham
        var phieuKham = new PhieuKham
        {
            MaLichKham = appointment.MaLichKham,
            MaBenhNhan = appointment.MaBenhNhan,
            MaBacSi = appointment.MaBacSi,
            TrieuChung = appointment.LyDoKham,
            NgayKham = DateTime.Now
        };

        ViewBag.Appointment = appointment;
        return View(phieuKham);
    }

    [HttpPost]
    public async Task<IActionResult> SaveConsultation(PhieuKham model)
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
                // Đổi trạng thái lịch khám
                appointment.TrangThai = "DaXacNhan"; // Có thể là "Đã khám"
                
                // Lưu Phiếu khám
                model.NgayKham = DateTime.Now;
                await _context.PhieuKhams.AddAsync(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Lưu kết quả khám bệnh thành công!";
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Chẩn đoán.";
        return RedirectToAction("Consultation", new { id = model.MaLichKham });
    }

    public async Task<IActionResult> History()
    {
        var docId = GetCurrentDoctorId();
        
        var history = await _context.PhieuKhams
            .Include(p => p.BenhNhan)
            .Where(p => p.MaBacSi == docId)
            .OrderByDescending(p => p.NgayKham)
            .Take(50)
            .ToListAsync();

        return View(history);
    }

    public async Task<IActionResult> Appointments(string status = "all")
    {
        var docId = GetCurrentDoctorId();
        ViewBag.CurrentStatus = status;

        var query = _context.LichKhams
            .Include(l => l.BenhNhan)
            .Where(l => l.MaBacSi == docId)
            .AsQueryable();

        if (status == "pending")
        {
            query = query.Where(l => l.TrangThai == "ChoXacNhan");
        }
        else if (status == "approved")
        {
            query = query.Where(l => l.TrangThai == "DaXacNhan");
        }
        
        var list = await query
            .OrderByDescending(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> MonthlySchedule(int? month, int? year)
    {
        var docId = GetCurrentDoctorId();
        
        var currentYear = year ?? DateTime.Today.Year;
        var currentMonth = month ?? DateTime.Today.Month;
        
        // Wrap around month/year logic
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
}
