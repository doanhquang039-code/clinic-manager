using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ManagerController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManagerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Tổng số thực từ DB
        var totalBenhNhan = await _context.BenhNhans.CountAsync();
        var totalBacSi = await _context.NguoiDungs.CountAsync(n => n.Role == "BacSi");
        var totalLichKham = await _context.LichKhams.CountAsync();
        var lichHomNay = await _context.LichKhams.CountAsync(l => l.NgayKham.Date == DateTime.Today);
        var lichHoanTat = await _context.LichKhams.CountAsync(l => l.TrangThai == "DaXacNhan");
        var lichChoXacNhan = await _context.LichKhams.CountAsync(l => l.TrangThai == "ChoXacNhan");

        // Doanh thu
        var totalRevenue = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan")
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;

        var revenueThisMonth = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan"
                        && h.NgayLap.HasValue
                        && h.NgayLap.Value.Month == DateTime.Now.Month
                        && h.NgayLap.Value.Year == DateTime.Now.Year)
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;

        // Lịch khám 7 ngày gần nhất (cho biểu đồ)
        var weeklyData = new List<int>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            var count = await _context.LichKhams.CountAsync(l => l.NgayKham.Date == date);
            weeklyData.Add(count);
        }

        // Lịch khám mới nhất
        var recentAppointments = await _context.LichKhams
            .Include(l => l.BenhNhan)
            .Include(l => l.BacSi)
            .OrderByDescending(l => l.NgayTao)
            .Take(8)
            .ToListAsync();

        ViewBag.TotalBenhNhan = totalBenhNhan;
        ViewBag.TotalBacSi = totalBacSi;
        ViewBag.TotalLichKham = totalLichKham;
        ViewBag.LichHomNay = lichHomNay;
        ViewBag.LichHoanTat = lichHoanTat;
        ViewBag.LichChoXacNhan = lichChoXacNhan;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.RevenueThisMonth = revenueThisMonth;
        ViewBag.WeeklyData = weeklyData;

        return View(recentAppointments);
    }

    public async Task<IActionResult> Personnel(string? search, string? roleFilter)
    {
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentRole"] = roleFilter;

        // Manager chỉ được quản lý các role cấp dưới (BacSi, ThuNgan, LeTan)
        // Không hiển thị Admin và Manager khác
        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };

        var query = _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => allowedRoles.Contains(n.Role))
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(n => n.HoTen.Contains(search) || n.Email.Contains(search) || n.SoDienThoai.Contains(search));

        if (!string.IsNullOrEmpty(roleFilter) && allowedRoles.Contains(roleFilter))
            query = query.Where(n => n.Role == roleFilter);

        var staff = await query.OrderBy(n => n.Role).ThenBy(n => n.HoTen).ToListAsync();
        ViewBag.Specialties = await _context.ChuyenKhoas.ToListAsync();
        return View(staff);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(NguoiDung model)
    {
        ModelState.Remove("ChuyenKhoa");
        ModelState.Remove("LichKhams");
        ModelState.Remove("PhieuKhams");
        ModelState.Remove("HoaDons");

        if (string.IsNullOrEmpty(model.MatKhau))
            model.MatKhau = "123456";
        model.NgayTao = DateTime.Now;
        model.TrangThai = true;

        await _context.NguoiDungs.AddAsync(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã thêm nhân viên {model.HoTen} thành công!";
        return RedirectToAction(nameof(Personnel));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStaffStatus(int id, bool status)
    {
        // Chỉ cho phép Manager thảo tác trên các role cấp dưới
        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };
        var staff = await _context.NguoiDungs.FindAsync(id);
        if (staff != null && allowedRoles.Contains(staff.Role))
        {
            staff.TrangThai = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã {(status ? "kích hoạt" : "khóa")} tài khoản {staff.HoTen}.";
        }
        else if (staff != null && !allowedRoles.Contains(staff.Role))
        {
            TempData["ErrorMessage"] = "Bạn không có quyền thảo tác với tài khoản này!";
        }
        return RedirectToAction(nameof(Personnel));
    }

    public async Task<IActionResult> Patients(string? search)
    {
        ViewData["CurrentSearch"] = search;

        var query = _context.BenhNhans.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.HoTen.Contains(search)
                || (b.SoDienThoai != null && b.SoDienThoai.Contains(search))
                || (b.Email != null && b.Email.Contains(search)));

        var patients = await query.OrderByDescending(b => b.NgayTao).ToListAsync();
        return View(patients);
    }

    public async Task<IActionResult> Invoices(string? status)
    {
        ViewData["CurrentStatus"] = status;

        var query = _context.HoaDons
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.NguoiLap)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(h => h.TrangThaiThanhToan == status);

        var invoices = await query.OrderByDescending(h => h.NgayLap).Take(100).ToListAsync();
        return View(invoices);
    }

    public async Task<IActionResult> Reports()
    {
        // Doanh thu theo tháng (6 tháng gần nhất)
        var monthlyRevenue = new List<decimal>();
        var monthLabels = new List<string>();

        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var rev = await _context.HoaDons
                .Where(h => h.TrangThaiThanhToan == "DaThanhToan"
                            && h.NgayLap.HasValue
                            && h.NgayLap.Value.Month == date.Month
                            && h.NgayLap.Value.Year == date.Year)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            monthlyRevenue.Add(rev);
            monthLabels.Add(date.ToString("MM/yyyy"));
        }

        // Lịch khám theo chuyên khoa
        var bySpecialty = await _context.LichKhams
            .Include(l => l.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .Where(l => l.BacSi!.ChuyenKhoa != null)
            .GroupBy(l => l.BacSi!.ChuyenKhoa!.TenChuyenKhoa)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.MonthlyRevenue = monthlyRevenue;
        ViewBag.MonthLabels = monthLabels;
        ViewBag.BySpecialty = bySpecialty;

        // Summary numbers
        ViewBag.TotalPatients = await _context.BenhNhans.CountAsync();
        ViewBag.TotalAppointments = await _context.LichKhams.CountAsync();
        ViewBag.TotalRevenue = await _context.HoaDons.Where(h => h.TrangThaiThanhToan == "DaThanhToan").SumAsync(h => (decimal?)h.TongTien) ?? 0;
        ViewBag.TotalDoctors = await _context.NguoiDungs.CountAsync(n => n.Role == "BacSi");

        return View();
    }

    public async Task<IActionResult> Schedules(DateTime? date, int? doctorId)
    {
        var selectedDate = date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate;
        ViewBag.Doctors = await _context.NguoiDungs
            .Where(n => n.Role == "BacSi" && n.TrangThai)
            .OrderBy(n => n.HoTen)
            .ToListAsync();
        ViewBag.SelectedDoctorId = doctorId;

        var query = _context.LichKhams
            .Include(l => l.BenhNhan)
            .Include(l => l.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .AsQueryable();

        query = query.Where(l => l.NgayKham.Date == selectedDate.Date);
        if (doctorId.HasValue)
            query = query.Where(l => l.MaBacSi == doctorId.Value);

        var appointments = await query.OrderBy(l => l.GioKham).ToListAsync();
        return View(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveAppointment(int id, string action)
    {
        var appointment = await _context.LichKhams.FindAsync(id);
        if (appointment != null)
        {
            appointment.TrangThai = action == "approve" ? "DaXacNhan" : "DaHuy";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = action == "approve"
                ? $"Đã xác nhận lịch khám #{id}."
                : $"Đã hủy lịch khám #{id}.";
        }
        return RedirectToAction(nameof(Schedules));
    }

    // ============ QUẢN LÝ KHO DƯỢC ============
    public async Task<IActionResult> DrugStock(string search, string statusFilter)
    {
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = statusFilter;

        var query = _context.Thuocs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.TenThuoc.Contains(search));

        if (statusFilter == "low")
            query = query.Where(t => t.SoLuongTon < 20);
        else if (statusFilter == "expired")
            query = query.Where(t => t.HanSuDung.HasValue && t.HanSuDung.Value.Date < DateTime.Today);
        else if (statusFilter == "outofstock")
            query = query.Where(t => t.TrangThai == "HetHang");

        var list = await query.OrderBy(t => t.TenThuoc).ToListAsync();
        
        ViewBag.TotalItems = await _context.Thuocs.CountAsync();
        ViewBag.LowStockCount = await _context.Thuocs.CountAsync(t => t.SoLuongTon < 20 && t.SoLuongTon > 0);
        ViewBag.OutOfStockCount = await _context.Thuocs.CountAsync(t => t.SoLuongTon == 0);
        ViewBag.NearExpiredCount = await _context.Thuocs.CountAsync(t => t.HanSuDung.HasValue && t.HanSuDung.Value.Date <= DateTime.Today.AddDays(30) && t.HanSuDung.Value.Date >= DateTime.Today);

        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDrug(string TenThuoc, string DonViTinh, decimal DonGia, int SoLuongTon, DateTime? HanSuDung)
    {
        if (!string.IsNullOrWhiteSpace(TenThuoc))
        {
            var thuoc = new Thuoc
            {
                TenThuoc = TenThuoc,
                DonViTinh = DonViTinh ?? "Viên",
                DonGia = DonGia,
                SoLuongTon = SoLuongTon,
                HanSuDung = HanSuDung,
                TrangThai = SoLuongTon > 0 ? "ConHang" : "HetHang"
            };
            _context.Thuocs.Add(thuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm thuốc vào kho thành công!";
        }
        return RedirectToAction(nameof(DrugStock));
    }

    [HttpPost]
    public async Task<IActionResult> EditDrug(int id, string TenThuoc, string DonViTinh, decimal DonGia, int SoLuongTon, DateTime? HanSuDung)
    {
        var thuoc = await _context.Thuocs.FindAsync(id);
        if (thuoc != null)
        {
            thuoc.TenThuoc = TenThuoc;
            thuoc.DonViTinh = DonViTinh;
            thuoc.DonGia = DonGia;
            thuoc.SoLuongTon = SoLuongTon;
            thuoc.HanSuDung = HanSuDung;
            thuoc.TrangThai = SoLuongTon > 0 ? "ConHang" : "HetHang";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật thuốc thành công!";
        }
        return RedirectToAction(nameof(DrugStock));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDrug(int id)
    {
        var thuoc = await _context.Thuocs.Include(t => t.ChiTietDonThuocs).FirstOrDefaultAsync(t => t.MaThuoc == id);
        if (thuoc != null && !thuoc.ChiTietDonThuocs.Any())
        {
            _context.Thuocs.Remove(thuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa thuốc thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể xóa thuốc đã được kê trong đơn thuốc!";
        }
        return RedirectToAction(nameof(DrugStock));
    }

    [HttpPost]
    public async Task<IActionResult> RestockDrug(int id, int SoLuongNhap)
    {
        var thuoc = await _context.Thuocs.FindAsync(id);
        if (thuoc != null && SoLuongNhap > 0)
        {
            thuoc.SoLuongTon += SoLuongNhap;
            thuoc.TrangThai = "ConHang";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã nhập {SoLuongNhap} {thuoc.DonViTinh} thuốc {thuoc.TenThuoc} vào kho!";
        }
        return RedirectToAction(nameof(DrugStock));
    }
}

