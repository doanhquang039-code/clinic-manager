using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "ThuNgan")]
public class CashierController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Invoices()
    {
        return View("ComingSoon");
    }

    public IActionResult History()
    {
        return View("ComingSoon");
    }
}
