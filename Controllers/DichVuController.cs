using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin,Manager,LeTan,BacSi")]
public class DichVuController : Controller
{
    private readonly IRepository<DichVu> _dichVuRepository;

    public DichVuController(IRepository<DichVu> dichVuRepository)
    {
        _dichVuRepository = dichVuRepository;
    }

    // GET: DichVu
    public async Task<IActionResult> Index()
    {
        var list = await _dichVuRepository.GetAllAsync();
        return View(list);
    }

    // GET: DichVu/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var dichVu = await _dichVuRepository.GetByIdAsync(id.Value);
        if (dichVu == null)
            return NotFound();

        return View(dichVu);
    }

    // GET: DichVu/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DichVu/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenDichVu,DonGia,MoTa")] DichVu dichVu)
    {
        if (ModelState.IsValid)
        {
            await _dichVuRepository.AddAsync(dichVu);
            return RedirectToAction(nameof(Index));
        }
        return View(dichVu);
    }

    // GET: DichVu/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var dichVu = await _dichVuRepository.GetByIdAsync(id.Value);
        if (dichVu == null)
            return NotFound();
            
        return View(dichVu);
    }

    // POST: DichVu/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaDichVu,TenDichVu,DonGia,MoTa")] DichVu dichVu)
    {
        if (id != dichVu.MaDichVu)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _dichVuRepository.UpdateAsync(dichVu);
            }
            catch (Exception)
            {
                if (!await _dichVuRepository.ExistsAsync(dichVu.MaDichVu))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(dichVu);
    }

    // GET: DichVu/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var dichVu = await _dichVuRepository.GetByIdAsync(id.Value);
        if (dichVu == null)
            return NotFound();

        return View(dichVu);
    }

    // POST: DichVu/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _dichVuRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
