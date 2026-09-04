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
        // Tá»•ng sá»‘ thá»±c tá»« DB
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

        // Lá»‹ch khÃ¡m 7 ngÃ y gáº§n nháº¥t (cho biá»ƒu Ä‘á»“)
        var weeklyData = new List<int>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            var count = await _context.LichKhams.CountAsync(l => l.NgayKham.Date == date);
            weeklyData.Add(count);
        }

        // Lá»‹ch khÃ¡m má»›i nháº¥t
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

    public async Task<IActionResult> Personnel(string? search, string? roleFilter, int page = 1)
    {
        const int pageSize = 10;
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentRole"] = roleFilter;

        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };

        var query = _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => allowedRoles.Contains(n.Role))
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(n => n.HoTen.Contains(search) || n.Email.Contains(search) || (n.SoDienThoai != null && n.SoDienThoai.Contains(search)));

        if (!string.IsNullOrEmpty(roleFilter) && allowedRoles.Contains(roleFilter))
            query = query.Where(n => n.Role == roleFilter);

        var totalCount = await query.CountAsync();
        var staff = await query.OrderBy(n => n.Role).ThenBy(n => n.HoTen)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.PageIndex = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalStaff = await _context.NguoiDungs.CountAsync(n => allowedRoles.Contains(n.Role));
        ViewBag.ActiveStaff = await _context.NguoiDungs.CountAsync(n => allowedRoles.Contains(n.Role) && n.TrangThai);
        ViewBag.InactiveStaff = await _context.NguoiDungs.CountAsync(n => allowedRoles.Contains(n.Role) && !n.TrangThai);
        ViewBag.DoctorCount = await _context.NguoiDungs.CountAsync(n => n.Role == "BacSi");
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
        TempData["SuccessMessage"] = $"ÄÃ£ thÃªm nhÃ¢n viÃªn {model.HoTen} thÃ nh cÃ´ng!";
        return RedirectToAction(nameof(Personnel));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStaffStatus(int id, bool status)
    {
        // Chá»‰ cho phÃ©p Manager tháº£o tÃ¡c trÃªn cÃ¡c role cáº¥p dÆ°á»›i
        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };
        var staff = await _context.NguoiDungs.FindAsync(id);
        if (staff != null && allowedRoles.Contains(staff.Role))
        {
            staff.TrangThai = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"ÄÃ£ {(status ? "kÃ­ch hoáº¡t" : "khÃ³a")} tÃ i khoáº£n {staff.HoTen}.";
        }
        else if (staff != null && !allowedRoles.Contains(staff.Role))
        {
            TempData["ErrorMessage"] = "Báº¡n khÃ´ng cÃ³ quyá»n tháº£o tÃ¡c vá»›i tÃ i khoáº£n nÃ y!";
        }
        return RedirectToAction(nameof(Personnel));
    }

    public async Task<IActionResult> Patients(string? search, int page = 1)
    {
        const int pageSize = 10;
        ViewData["CurrentSearch"] = search;

        var query = _context.BenhNhans.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.HoTen.Contains(search)
                || (b.SoDienThoai != null && b.SoDienThoai.Contains(search))
                || (b.Email != null && b.Email.Contains(search))
                || (b.CCCD != null && b.CCCD.Contains(search)));

        var totalCount = await query.CountAsync();
        var patients = await query.OrderByDescending(b => b.NgayTao)
            .Skip((page-1)*pageSize).Take(pageSize).ToListAsync();

        // Láº¥y tráº¡ng thÃ¡i hÃ³a Ä‘Æ¡n má»›i nháº¥t cá»§a má»—i bá»‡nh nhÃ¢n
        var patientIds = patients.Select(p => p.MaBenhNhan).ToList();
        var invoiceStatus = await _context.HoaDons
            .Include(h => h.PhieuKham)
            .Where(h => h.PhieuKham != null && patientIds.Contains(h.PhieuKham.MaBenhNhan))
            .GroupBy(h => h.PhieuKham!.MaBenhNhan)
            .Select(g => new { PatientId = g.Key, Status = g.OrderByDescending(x => x.NgayLap).First().TrangThaiThanhToan })
            .ToDictionaryAsync(x => x.PatientId, x => x.Status);

        ViewBag.PatientInvoiceStatus = invoiceStatus;
        ViewBag.PageIndex = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        return View(patients);
    }

    public async Task<IActionResult> Invoices(string? status, int page = 1)
    {
        const int pageSize = 15;
        ViewData["CurrentStatus"] = status;

        var query = _context.HoaDons
            .Include(h => h.PhieuKham).ThenInclude(p => p!.BenhNhan)
            .Include(h => h.NguoiLap)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(h => h.TrangThaiThanhToan == status);

        var totalCount = await query.CountAsync();
        var invoices = await query.OrderByDescending(h => h.NgayLap)
            .Skip((page-1)*pageSize).Take(pageSize).ToListAsync();

        var thisMonth = DateTime.Now;
        ViewBag.RevenueThisMonth = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.NgayLap.HasValue
                && h.NgayLap.Value.Month == thisMonth.Month && h.NgayLap.Value.Year == thisMonth.Year)
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;
        ViewBag.CashRevenue = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.PhuongThucThanhToan == "TienMat"
                && h.NgayLap.HasValue && h.NgayLap.Value.Month == thisMonth.Month)
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;
        ViewBag.TransferRevenue = await _context.HoaDons
            .Where(h => h.TrangThaiThanhToan == "DaThanhToan" && h.PhuongThucThanhToan == "ChuyenKhoan"
                && h.NgayLap.HasValue && h.NgayLap.Value.Month == thisMonth.Month)
            .SumAsync(h => (decimal?)h.TongTien) ?? 0;
        ViewBag.PendingCount = await _context.HoaDons.CountAsync(h => h.TrangThaiThanhToan == "ChuaThanhToan");
        ViewBag.PageIndex = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        return View(invoices);
    }

    public async Task<IActionResult> Schedules(DateTime? date, int? doctorId)
    {
        var selectedDate = date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate;
        ViewBag.Doctors = await _context.NguoiDungs
            .Where(n => n.Role == "BacSi" && n.TrangThai)
            .OrderBy(n => n.HoTen)
            .ToListAsync();
        ViewBag.Patients = await _context.BenhNhans
            .OrderBy(b => b.HoTen)
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

    [HttpPost]
    public async Task<IActionResult> CreateDrug(string TenThuoc, string DonViTinh, decimal DonGia, int SoLuongTon, DateTime? HanSuDung)
    {
        if (!string.IsNullOrWhiteSpace(TenThuoc))
        {
            var thuoc = new Thuoc
            {
                TenThuoc = TenThuoc,
                DonViTinh = DonViTinh ?? "ViÃªn",
                DonGia = DonGia,
                SoLuongTon = SoLuongTon,
                HanSuDung = HanSuDung,
                TrangThai = SoLuongTon > 0 ? "ConHang" : "HetHang"
            };
            _context.Thuocs.Add(thuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ThÃªm thuá»‘c vÃ o kho thÃ nh cÃ´ng!";
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
            TempData["SuccessMessage"] = "Cáº­p nháº­t thuá»‘c thÃ nh cÃ´ng!";
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
            TempData["SuccessMessage"] = "XÃ³a thuá»‘c thÃ nh cÃ´ng!";
        }
        else
        {
            TempData["ErrorMessage"] = "KhÃ´ng thá»ƒ xÃ³a thuá»‘c Ä‘Ã£ Ä‘Æ°á»£c kÃª trong Ä‘Æ¡n thuá»‘c!";
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
            TempData["SuccessMessage"] = $"ÄÃ£ nháº­p {SoLuongNhap} {thuoc.DonViTinh} thuá»‘c {thuoc.TenThuoc} vÃ o kho!";
        }
        return RedirectToAction(nameof(DrugStock));
    }

    // ============ CHá»ˆNH Sá»¬A NHÃ‚N VIÃŠN ============
    [HttpPost]
    public async Task<IActionResult> EditStaff(int id, string HoTen, string? SoDienThoai, string? Email, int? MaChuyenKhoa)
    {
        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };
        var staff = await _context.NguoiDungs.FindAsync(id);
        if (staff != null && allowedRoles.Contains(staff.Role))
        {
            staff.HoTen = HoTen;
            staff.SoDienThoai = SoDienThoai;
            staff.Email = Email;
            if (staff.Role == "BacSi")
                staff.MaChuyenKhoa = MaChuyenKhoa;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"ÄÃ£ cáº­p nháº­t thÃ´ng tin {staff.HoTen} thÃ nh cÃ´ng!";
        }
        return RedirectToAction(nameof(Personnel));
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var allowedRoles = new[] { "BacSi", "ThuNgan", "LeTan" };
        var staff = await _context.NguoiDungs.FindAsync(id);
        if (staff != null && allowedRoles.Contains(staff.Role))
        {
            staff.MatKhau = "123456";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"ÄÃ£ Ä‘áº·t láº¡i máº­t kháº©u cho {staff.HoTen} thÃ nh 123456.";
        }
        return RedirectToAction(nameof(Personnel));
    }

    // ============ CHI TIáº¾T HÃ“A ÄÆ N ============
    public async Task<IActionResult> InvoiceDetail(int id)
    {
        var invoice = await _context.HoaDons
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p!.BenhNhan)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p!.BacSi)
                    .ThenInclude(b => b!.ChuyenKhoa)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p!.ChiTietDonThuocs)
                    .ThenInclude(c => c.Thuoc)
            .Include(h => h.PhieuKham)
                .ThenInclude(p => p!.ChiTietDichVus)
                    .ThenInclude(c => c.DichVu)
            .Include(h => h.NguoiLap)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    // ============ Táº O Lá»ŠCH KHÃM ============
    [HttpPost]
    public async Task<IActionResult> CreateSchedule(int MaBenhNhan, int MaBacSi, DateTime NgayKham, TimeSpan GioKham, string? LyDoKham)
    {
        var lichKham = new LichKham
        {
            MaBenhNhan = MaBenhNhan,
            MaBacSi = MaBacSi,
            NgayKham = NgayKham,
            GioKham = GioKham,
            LyDoKham = LyDoKham,
            TrangThai = "DaXacNhan",
            NgayTao = DateTime.Now
        };
        _context.LichKhams.Add(lichKham);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"ÄÃ£ táº¡o lá»‹ch khÃ¡m thÃ nh cÃ´ng cho ngÃ y {NgayKham:dd/MM/yyyy} lÃºc {GioKham:hh\\:mm}!";
        return RedirectToAction(nameof(Schedules), new { date = NgayKham.ToString("yyyy-MM-dd") });
    }

    // ============ BÃO CÃO NÃ‚NG CAO ============
    public async Task<IActionResult> Reports()
    {
        // Doanh thu theo thÃ¡ng (6 thÃ¡ng gáº§n nháº¥t)
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

        // Lá»‹ch khÃ¡m theo chuyÃªn khoa
        var bySpecialty = await _context.LichKhams
            .Include(l => l.BacSi).ThenInclude(b => b!.ChuyenKhoa)
            .Where(l => l.BacSi!.ChuyenKhoa != null)
            .GroupBy(l => l.BacSi!.ChuyenKhoa!.TenChuyenKhoa)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        // Top 5 bÃ¡c sÄ© nhiá»u ca khÃ¡m nháº¥t trong thÃ¡ng hiá»‡n táº¡i
        var topDoctors = await _context.LichKhams
            .Include(l => l.BacSi)
            .Where(l => l.NgayKham.Month == DateTime.Now.Month && l.NgayKham.Year == DateTime.Now.Year)
            .GroupBy(l => new { l.MaBacSi, l.BacSi!.HoTen })
            .Select(g => new { DoctorId = g.Key.MaBacSi, Name = g.Key.HoTen, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // Thuá»‘c sáº¯p háº¿t hÃ ng (dÆ°á»›i 20 Ä‘Æ¡n vá»‹)
        var lowStockDrugs = await _context.Thuocs
            .Where(t => t.SoLuongTon < 20 && t.SoLuongTon > 0)
            .OrderBy(t => t.SoLuongTon)
            .Take(10)
            .ToListAsync();

        // Thuá»‘c háº¿t hÃ ng
        var outOfStockDrugs = await _context.Thuocs
            .Where(t => t.SoLuongTon == 0)
            .Take(10)
            .ToListAsync();

        // Thá»‘ng kÃª lá»‹ch khÃ¡m theo tráº¡ng thÃ¡i thÃ¡ng nÃ y
        var thisMonthTotal = await _context.LichKhams
            .CountAsync(l => l.NgayKham.Month == DateTime.Now.Month && l.NgayKham.Year == DateTime.Now.Year);
        var thisMonthDone = await _context.LichKhams
            .CountAsync(l => l.NgayKham.Month == DateTime.Now.Month && l.NgayKham.Year == DateTime.Now.Year && l.TrangThai == "DaKham");
        var thisMonthCancelled = await _context.LichKhams
            .CountAsync(l => l.NgayKham.Month == DateTime.Now.Month && l.NgayKham.Year == DateTime.Now.Year && l.TrangThai == "DaHuy");

        ViewBag.MonthlyRevenue = monthlyRevenue;
        ViewBag.MonthLabels = monthLabels;
        ViewBag.BySpecialty = bySpecialty;
        ViewBag.TopDoctors = topDoctors;
        ViewBag.LowStockDrugs = lowStockDrugs;
        ViewBag.OutOfStockDrugs = outOfStockDrugs;
        ViewBag.ThisMonthTotal = thisMonthTotal;
        ViewBag.ThisMonthDone = thisMonthDone;
        ViewBag.ThisMonthCancelled = thisMonthCancelled;

        ViewBag.TotalPatients = await _context.BenhNhans.CountAsync();
        ViewBag.TotalAppointments = await _context.LichKhams.CountAsync();
        ViewBag.TotalRevenue = await _context.HoaDons.Where(h => h.TrangThaiThanhToan == "DaThanhToan").SumAsync(h => (decimal?)h.TongTien) ?? 0;
        ViewBag.TotalDoctors = await _context.NguoiDungs.CountAsync(n => n.Role == "BacSi");

        return View();
    }
    // ============ KHO THUá»C & Váº¬T TÆ¯ ============
    public async Task<IActionResult> DrugStock(string? search, string? statusFilter, int page = 1)
    {
        const int pageSize = 15;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = statusFilter;

        var query = _context.Thuocs.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.TenThuoc.Contains(search) || t.MaThuoc.ToString().Contains(search));

        if (statusFilter == "low") query = query.Where(t => t.SoLuongTon > 0 && t.SoLuongTon < 20);
        else if (statusFilter == "outofstock") query = query.Where(t => t.SoLuongTon == 0);
        else if (statusFilter == "expired") query = query.Where(t => t.HanSuDung.HasValue && t.HanSuDung.Value < DateTime.Today);

        var totalCount = await query.CountAsync();
        var drugs = await query.OrderBy(t => t.TenThuoc)
            .Skip((page-1)*pageSize).Take(pageSize).ToListAsync();

        ViewBag.PageIndex = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalItems = await _context.Thuocs.CountAsync();
        ViewBag.StockValue = await _context.Thuocs.SumAsync(t => (decimal?)t.DonGia * t.SoLuongTon) ?? 0;
        ViewBag.LowStockCount = await _context.Thuocs.CountAsync(t => t.SoLuongTon > 0 && t.SoLuongTon < 20);
        ViewBag.NearExpiredCount = await _context.Thuocs.CountAsync(t => t.HanSuDung.HasValue && t.HanSuDung.Value < DateTime.Today.AddMonths(3) && t.HanSuDung.Value >= DateTime.Today);
        return View(drugs);
    }
}
