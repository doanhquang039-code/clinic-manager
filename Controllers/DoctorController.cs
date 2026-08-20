using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "BacSi")]
public class DoctorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Consultation()
    {
        return View("ComingSoon");
    }

    public IActionResult History()
    {
        return View("ComingSoon");
    }
}
