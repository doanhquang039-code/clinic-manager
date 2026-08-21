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

        return View(todayAppointments);
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
}
