using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Manager"))
                return RedirectToAction("Index", "Manager");
            if (User.IsInRole("BacSi"))
                return RedirectToAction("Index", "Doctor");
            if (User.IsInRole("LeTan"))
                return RedirectToAction("Index", "Staff");
            if (User.IsInRole("ThuNgan"))
                return RedirectToAction("Index", "Cashier");
            if (User.IsInRole("BenhNhan"))
                return RedirectToAction("Index", "Patient");
                
            return RedirectToAction("Index", "Patient"); // Fallback
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
