using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Models;
using MyMvcApp.Repositories;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin,Manager,LeTan,BacSi")]
public class PhieuKhamController : Controller
{
    private readonly IRepository<PhieuKham> _phieuKhamRepository;
    private readonly IRepository<LichKham> _lichKhamRepository;
    private readonly IRepository<BenhNhan> _benhNhanRepository;
    private readonly IRepository<NguoiDung> _nguoiDungRepository;

    public PhieuKhamController(
        IRepository<PhieuKham> phieuKhamRepository,
        IRepository<LichKham> lichKhamRepository,
        IRepository<BenhNhan> benhNhanRepository,
        IRepository<NguoiDung> nguoiDungRepository)
    {
        _phieuKhamRepository = phieuKhamRepository;
        _lichKhamRepository = lichKhamRepository;
        _benhNhanRepository = benhNhanRepository;
        _nguoiDungRepository = nguoiDungRepository;
    }

    public async Task<IActionResult> Index()
    {
        var phieuKhams = await _phieuKhamRepository.GetAllAsync(p => p.BenhNhan, p => p.BacSi);
        return View(phieuKhams.OrderByDescending(p => p.NgayKham));
    }

    // Nút tạo Phiếu Khám từ Lịch Khám
    [HttpGet]
    public async Task<IActionResult> CreateFromLich(int maLichKham)
    {
        var lichKham = await _lichKhamRepository.GetByIdAsync(maLichKham);
        if (lichKham == null) return NotFound();

        var viewModel = new PhieuKhamViewModel
        {
            PhieuKham = new PhieuKham
            {
                MaLichKham = lichKham.MaLichKham,
                MaBenhNhan = lichKham.MaBenhNhan,
                MaBacSi = lichKham.MaBacSi,
                NgayKham = DateTime.Now,
                TrieuChung = lichKham.LyDoKham
            },
            BenhNhanList = await GetBenhNhanSelectList(),
            BacSiList = await GetBacSiSelectList(),
            LichKhamList = await GetLichKhamSelectList()
        };
        
        return View("Create", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new PhieuKhamViewModel
        {
            PhieuKham = new PhieuKham { NgayKham = DateTime.Now },
            BenhNhanList = await GetBenhNhanSelectList(),
            BacSiList = await GetBacSiSelectList(),
            LichKhamList = await GetLichKhamSelectList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhieuKhamViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            await _phieuKhamRepository.AddAsync(viewModel.PhieuKham);

            // Cập nhật trạng thái Lịch khám thành "Đã khám"
            if (viewModel.PhieuKham.MaLichKham.HasValue)
            {
                var lich = await _lichKhamRepository.GetByIdAsync(viewModel.PhieuKham.MaLichKham.Value);
                if (lich != null)
                {
                    lich.TrangThai = "DaKham";
                    await _lichKhamRepository.UpdateAsync(lich);
                }
            }

            TempData["SuccessMessage"] = "Tạo Phiếu Khám thành công.";
            return RedirectToAction(nameof(Index));
        }

        viewModel.BenhNhanList = await GetBenhNhanSelectList();
        viewModel.BacSiList = await GetBacSiSelectList();
        viewModel.LichKhamList = await GetLichKhamSelectList();
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var phieuKham = await _phieuKhamRepository.GetByIdAsync(id);
        if (phieuKham == null) return NotFound();

        var viewModel = new PhieuKhamViewModel
        {
            PhieuKham = phieuKham,
            BenhNhanList = await GetBenhNhanSelectList(),
            BacSiList = await GetBacSiSelectList(),
            LichKhamList = await GetLichKhamSelectList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PhieuKhamViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            await _phieuKhamRepository.UpdateAsync(viewModel.PhieuKham);
            TempData["SuccessMessage"] = "Cập nhật Phiếu Khám thành công.";
            return RedirectToAction(nameof(Index));
        }
        viewModel.BenhNhanList = await GetBenhNhanSelectList();
        viewModel.BacSiList = await GetBacSiSelectList();
        viewModel.LichKhamList = await GetLichKhamSelectList();
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var phieuKham = await _phieuKhamRepository.GetByIdAsync(id, p => p.BenhNhan, p => p.BacSi);
        if (phieuKham == null) return NotFound();
        return View(phieuKham);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var phieuKham = await _phieuKhamRepository.GetByIdAsync(id, p => p.BenhNhan, p => p.BacSi);
        if (phieuKham == null) return NotFound();
        return View(phieuKham);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _phieuKhamRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Xóa Phiếu Khám thành công.";
        return RedirectToAction(nameof(Index));
    }

    // --- Helper Methods ---
    private async Task<IEnumerable<SelectListItem>> GetBenhNhanSelectList()
    {
        var items = await _benhNhanRepository.GetAllAsync();
        return items.Select(b => new SelectListItem { Value = b.MaBenhNhan.ToString(), Text = $"{b.HoTen} ({b.SoDienThoai})" });
    }

    private async Task<IEnumerable<SelectListItem>> GetBacSiSelectList()
    {
        var items = await _nguoiDungRepository.FindAsync(n => n.Role == "BacSi");
        return items.Select(b => new SelectListItem { Value = b.MaNguoiDung.ToString(), Text = b.HoTen });
    }

    private async Task<IEnumerable<SelectListItem>> GetLichKhamSelectList()
    {
        var items = await _lichKhamRepository.GetAllAsync(l => l.BenhNhan);
        return items.Select(l => new SelectListItem 
        { 
            Value = l.MaLichKham.ToString(), 
            Text = $"Lịch {l.MaLichKham} - {l.BenhNhan?.HoTen} - {l.NgayKham:dd/MM/yyyy}" 
        });
    }
}
