using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[Authorize(Roles = "Hospital")]
public sealed class HospitalController : Controller
{
    private readonly ApiClient _apiClient;

    public HospitalController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _apiClient.GetAsync<HospitalProfileViewModel>("api/hospitals/me", cancellationToken);
            var requests = await _apiClient.GetAsync<List<BloodRequestViewModel>>("api/bloodrequests", cancellationToken);
            return View(new HospitalDashboardViewModel
            {
                Profile = profile,
                Requests = requests.Where(request => request.HospitalId == profile.Id).ToList()
            });
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new HospitalDashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _apiClient.GetAsync<HospitalProfileViewModel>("api/hospitals/me", cancellationToken));
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new HospitalProfileViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateHospitalProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.PutAsync<HospitalProfileViewModel>("api/hospitals/me", model, cancellationToken);
            TempData["Success"] = "Hospital profile updated.";
            return RedirectToAction(nameof(Profile));
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int requestId, string status, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.PutAsync<BloodRequestViewModel>(
                $"api/bloodrequests/{requestId}/status",
                new { Status = status },
                cancellationToken);
            TempData["Success"] = $"Request updated to {status}.";
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
