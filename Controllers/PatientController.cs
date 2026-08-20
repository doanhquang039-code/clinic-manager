using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "BenhNhan")]
public class PatientController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Home()
    {
        return View("ComingSoon");
    }

    public IActionResult BookAppointment()
    {
        return View("ComingSoon");
    }

    public IActionResult MedicalRecords()
    {
        return View("ComingSoon");
    }
}
