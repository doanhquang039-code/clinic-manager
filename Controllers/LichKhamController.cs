using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Models;
using MyMvcApp.Repositories;
using System.Security.Claims;

namespace MyMvcApp.Controllers;

[Authorize]
public class LichKhamController : Controller
{
    private readonly IRepository<LichKham> _lichKhamRepository;
    private readonly IRepository<BenhNhan> _benhNhanRepository;
    private readonly IRepository<NguoiDung> _nguoiDungRepository;

    public LichKhamController(
        IRepository<LichKham> lichKhamRepository,
        IRepository<BenhNhan> benhNhanRepository,
        IRepository<NguoiDung> nguoiDungRepository)
    {
        _lichKhamRepository = lichKhamRepository;
        _benhNhanRepository = benhNhanRepository;
        _nguoiDungRepository = nguoiDungRepository;
    }

    public async Task<IActionResult> Index()
    {
        var allLichKham = await _lichKhamRepository.GetAllAsync(l => l.BenhNhan, l => l.BacSi);
        
        var userType = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;
        
        if (userType == "BenhNhan")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            allLichKham = allLichKham.Where(l => l.MaBenhNhan == userId).ToList();
        }
        else if (User.IsInRole("BacSi"))
        {
            var bacSiId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            // Bác sĩ có thể xem lịch của mình (hoặc Admin xem hết)
            // Để đơn giản, NguoiDung xem hết, nhưng nếu thích ta có thể lọc riêng cho BacSi
            // allLichKham = allLichKham.Where(l => l.MaBacSi == bacSiId).ToList();
        }

        return View(allLichKham.OrderByDescending(l => l.NgayKham).ThenByDescending(l => l.GioKham));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new LichKhamViewModel
        {
            LichKham = new LichKham { NgayKham = DateTime.Now.Date, GioKham = new TimeSpan(8, 0, 0) },
            BenhNhanList = await GetBenhNhanSelectList(),
            BacSiList = await GetBacSiSelectList()
        };

        var userType = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;
        if (userType == "BenhNhan")
        {
            viewModel.LichKham.MaBenhNhan = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LichKhamViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            await _lichKhamRepository.AddAsync(viewModel.LichKham);
            TempData["SuccessMessage"] = "Đăng ký Lịch Khám thành công.";
            return RedirectToAction(nameof(Index));
        }

        viewModel.BenhNhanList = await GetBenhNhanSelectList();
        viewModel.BacSiList = await GetBacSiSelectList();
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var lichKham = await _lichKhamRepository.GetByIdAsync(id, l => l.BenhNhan, l => l.BacSi);
        if (lichKham == null)
            return NotFound();

        // Kiểm tra quyền
        if (!CanAccess(lichKham.MaBenhNhan))
            return Forbid();

        return View(lichKham);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lichKham = await _lichKhamRepository.GetByIdAsync(id);
        if (lichKham == null)
            return NotFound();

        if (!CanAccess(lichKham.MaBenhNhan))
            return Forbid();

        var viewModel = new LichKhamViewModel
        {
            LichKham = lichKham,
            BenhNhanList = await GetBenhNhanSelectList(),
            BacSiList = await GetBacSiSelectList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LichKhamViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            await _lichKhamRepository.UpdateAsync(viewModel.LichKham);
            TempData["SuccessMessage"] = "Cập nhật Lịch Khám thành công.";
            return RedirectToAction(nameof(Index));
        }
        viewModel.BenhNhanList = await GetBenhNhanSelectList();
        viewModel.BacSiList = await GetBacSiSelectList();
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var lichKham = await _lichKhamRepository.GetByIdAsync(id, l => l.BenhNhan, l => l.BacSi);
        if (lichKham == null)
            return NotFound();

        if (!CanAccess(lichKham.MaBenhNhan))
            return Forbid();

        return View(lichKham);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _lichKhamRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Xóa Lịch Khám thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,LeTan,BacSi")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var lichKham = await _lichKhamRepository.GetByIdAsync(id);
        if (lichKham != null)
        {
            lichKham.TrangThai = status;
            await _lichKhamRepository.UpdateAsync(lichKham);
            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    // --- Helper Methods ---

    private bool CanAccess(int patientId)
    {
        var userType = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;
        if (userType == "NguoiDung") return true; // Admin/BacSi/LeTan có thể xem mọi lịch
        
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        return patientId == userId; // Bệnh nhân chỉ xem được lịch của mình
    }

    private async Task<IEnumerable<SelectListItem>> GetBenhNhanSelectList()
    {
        var benhNhans = await _benhNhanRepository.GetAllAsync();
        return benhNhans.Select(b => new SelectListItem
        {
            Value = b.MaBenhNhan.ToString(),
            Text = $"{b.HoTen} ({b.SoDienThoai})"
        });
    }

    private async Task<IEnumerable<SelectListItem>> GetBacSiSelectList()
    {
        // Chỉ lấy những NguoiDung có Role là BacSi
        var bacSis = await _nguoiDungRepository.FindAsync(n => n.Role == "BacSi");
        return bacSis.Select(b => new SelectListItem
        {
            Value = b.MaNguoiDung.ToString(),
            Text = b.HoTen
        });
    }
}
