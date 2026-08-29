using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

public sealed class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "AdminDashboard");
        if (User.IsInRole("Requester")) return RedirectToAction("Index", "Requester");
        if (User.IsInRole("Donor")) return RedirectToAction("Index", "Donor");
        if (User.IsInRole("Hospital")) return RedirectToAction("Index", "Hospital");
        return View();
    }
}
