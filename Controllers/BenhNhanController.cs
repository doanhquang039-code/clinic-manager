using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin,Manager,LeTan,BacSi")]
public class BenhNhanController : Controller
{
    private readonly IRepository<BenhNhan> _benhNhanRepository;

    public BenhNhanController(IRepository<BenhNhan> benhNhanRepository)
    {
        _benhNhanRepository = benhNhanRepository;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _benhNhanRepository.GetAllAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _benhNhanRepository.GetByIdAsync(id.Value);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BenhNhan benhNhan)
    {
        if (ModelState.IsValid)
        {
            await _benhNhanRepository.AddAsync(benhNhan);
            return RedirectToAction(nameof(Index));
        }
        return View(benhNhan);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _benhNhanRepository.GetByIdAsync(id.Value);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BenhNhan benhNhan)
    {
        if (id != benhNhan.MaBenhNhan) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _benhNhanRepository.UpdateAsync(benhNhan);
            }
            catch
            {
                if (!await _benhNhanRepository.ExistsAsync(benhNhan.MaBenhNhan))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(benhNhan);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _benhNhanRepository.GetByIdAsync(id.Value);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _benhNhanRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
