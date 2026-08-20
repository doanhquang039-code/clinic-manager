using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Personnel()
    {
        return View("ComingSoon");
    }

    public IActionResult Patients()
    {
        return View("ComingSoon");
    }

    public IActionResult Inventory()
    {
        return View("ComingSoon");
    }

    public IActionResult Invoices()
    {
        return View("ComingSoon");
    }

    public IActionResult Reports()
    {
        return View("ComingSoon");
    }
}
