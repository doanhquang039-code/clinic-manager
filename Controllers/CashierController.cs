using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "ThuNgan,Manager,Admin")]
public class CashierController : Controller
{
    private readonly ApplicationDbContext _context;

    public CashierController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out int id) ? id : 0;
    }

    // Dashboard: thống kê nhanh + hàng đợi thanh toán
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var pendingPayments = await _context.PhieuKhams
            .Include(p => p.BenhNhan)
            .Include(p => p.BacSi)
            .Where(p => p.HoaDon == null)   // Chưa lập hóa đơn
            .OrderByDescending(p => p.NgayKham)
            .ToListAsync();

        var todayRevenue = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan"
                        && h.NgayLap.HasValue
                        && h.NgayLap.Value.Date == today)
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;

        var todayCount = await _context.HoaDons
            .Where(h => h.NgayLap.HasValue && h.NgayLap.Value.Date == today)
            .CountAsync();

        var paidCount = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan"
                        && h.NgayLap.HasValue && h.NgayLap.Value.Date == today)
            .CountAsync();

        ViewBag.TodayRevenue = todayRevenue;
        ViewBag.TodayCount = todayCount;
        ViewBag.PaidCount = paidCount;
        ViewBag.PendingCount = pendingPayments.Count;

        return View(pendingPayments);
    }

    // Tạo hóa đơn cho 1 phiếu khám
    public async Task<IActionResult> CreateInvoice(int phieuKhamId)
    {
        var phieuKham = await _context.PhieuKhams
            .Include(p => p.BenhNhan)
            .Include(p => p.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .Include(p => p.ChiTietDichVus)
            .ThenInclude(c => c.DichVu)
            .FirstOrDefaultAsync(p => p.MaPhieuKham == phieuKhamId);

        if (phieuKham == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy phiếu khám.";
            return RedirectToAction(nameof(Index));
        }

        // Tính tổng tiền dịch vụ
        var tienDichVu = phieuKham.ChiTietDichVus.Sum(c => c.DichVu?.DonGia ?? 0);

        ViewBag.PhieuKham = phieuKham;
        ViewBag.TienDichVu = tienDichVu;

        var hoaDon = new HoaDon
        {
            MaPhieuKham = phieuKhamId,
            MaNguoiLap = GetCurrentUserId(),
            TienDichVu = tienDichVu,
            TienThuoc = 0,
            NgayLap = DateTime.Now
        };

        return View(hoaDon);
    }

    [HttpPost]
    public async Task<IActionResult> SaveInvoice(HoaDon model)
    {
        ModelState.Remove("PhieuKham");
        ModelState.Remove("NguoiLap");

        model.NgayLap = DateTime.Now;
        model.MaNguoiLap = GetCurrentUserId();
        model.TrangThaiThanhToan = "DaThanhToan";

        if (ModelState.IsValid)
        {
            await _context.HoaDons.AddAsync(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Hóa đơn HD-{model.MaHoaDon:D5} đã được lập thành công!";
            return RedirectToAction(nameof(Invoices));
        }

        TempData["ErrorMessage"] = "Lỗi khi lập hóa đơn. Vui lòng thử lại.";
        return RedirectToAction("CreateInvoice", new { phieuKhamId = model.MaPhieuKham });
    }

    // Danh sách hóa đơn đã lập
    public async Task<IActionResult> Invoices(string? status, string? search)
    {
        ViewData["CurrentStatus"] = status;
        ViewData["CurrentSearch"] = search;

        var query = _context.HoaDons
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.NguoiLap)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(h => h.TrangThaiThanhToan == status);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(h => h.PhieuKham!.BenhNhan!.HoTen.Contains(search));

        var invoices = await query.OrderByDescending(h => h.NgayLap).Take(100).ToListAsync();
        return View(invoices);
    }

    // Lịch sử giao dịch
    public async Task<IActionResult> History()
    {
        var history = await _context.HoaDons
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.NguoiLap)
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan")
            .OrderByDescending(h => h.NgayLap)
            .Take(100)
            .ToListAsync();

        return View(history);
    }

    // In hóa đơn - trang print-friendly
    public async Task<IActionResult> PrintInvoice(int id)
    {
        var invoice = await _context.HoaDons
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BacSi)
            .ThenInclude(b => b!.ChuyenKhoa)
            .Include(h => h.NguoiLap)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id);

        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
            return RedirectToAction(nameof(Invoices));
        }
        return View(invoice);
    }

    // Thống kê cuối ngày
    public async Task<IActionResult> DaySummary(DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;
        ViewBag.TargetDate = targetDate;

        var invoicesToday = await _context.HoaDons
            .Include(h => h.PhieuKham)
            .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.NguoiLap)
            .Where(h => h.NgayLap.HasValue && h.NgayLap.Value.Date == targetDate.Date)
            .OrderBy(h => h.NgayLap)
            .ToListAsync();

        ViewBag.TotalRevenue = invoicesToday.Where(h => h.TrangThaiThanhToan == "DaThanhToan").Sum(h => h.TongTien);
        ViewBag.TotalCount = invoicesToday.Count;
        ViewBag.PaidCount = invoicesToday.Count(h => h.TrangThaiThanhToan == "DaThanhToan");
        ViewBag.CashRevenue = invoicesToday.Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.PhuongThucThanhToan == "TienMat").Sum(h => h.TongTien);
        ViewBag.TransferRevenue = invoicesToday.Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.PhuongThucThanhToan == "ChuyenKhoan").Sum(h => h.TongTien);
        ViewBag.CardRevenue = invoicesToday.Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.PhuongThucThanhToan == "TheNganHang").Sum(h => h.TongTien);

        return View(invoicesToday);
    }
}
