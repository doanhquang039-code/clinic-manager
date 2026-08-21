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
        ViewBag.Doctors = await _context.NguoiDungs
            .Where(n => n.Role == "BacSi" && n.TrangThai)
            .Select(d => new { d.MaNguoiDung, d.HoTen, d.MaChuyenKhoa })
            .ToListAsync();
            
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
            .Include(p => p.LichKham)
            .Where(p => p.MaBenhNhan == patientId)
            .OrderByDescending(p => p.NgayKham)
            .ToListAsync();

        return View(records);
    }
}
