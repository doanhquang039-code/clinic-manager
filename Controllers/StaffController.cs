using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "LeTan")]
public class StaffController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Appointments()
    {
        return View("ComingSoon");
    }

    public IActionResult Tracking()
    {
        return View("ComingSoon");
    }
}
