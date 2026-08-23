using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "LeTan")]
public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        
        var dashboardData = new
        {
            TotalPatients = await _context.BenhNhans.CountAsync(),
            TodayAppointments = await _context.LichKhams.CountAsync(l => l.NgayKham == today),
            PendingAppointments = await _context.LichKhams.CountAsync(l => l.TrangThai == "ChoXacNhan"),
            ActiveDoctors = await _context.NguoiDungs.CountAsync(n => n.Role == "BacSi" && n.TrangThai)
        };

        ViewBag.DashboardData = dashboardData;
        return View();
    }

    public async Task<IActionResult> Appointments()
    {
        // Get pre-booked appointments (ChoXacNhan, DaXacNhan)
        var appointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Include(l => l.BacSi)
            .Where(l => l.TrangThai == "ChoXacNhan" || l.TrangThai == "DaXacNhan")
            .OrderBy(l => l.NgayKham)
            .ThenBy(l => l.GioKham)
            .ToListAsync();
        
        return View(appointments);
    }

    public async Task<IActionResult> Tracking()
    {
        // Get doctors currently active (TrangThai == 1)
        var doctors = await _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => n.Role == "BacSi" && n.TrangThai)
            .OrderBy(n => n.ChuyenKhoa.TenChuyenKhoa)
            .ToListAsync();

        return View(doctors);
    }

    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult Settings()
    {
        return View();
    }
}
