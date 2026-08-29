using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[Authorize(Roles = "Requester")]
public sealed class RequesterController : Controller
{
    private readonly ApiClient _apiClient;

    public RequesterController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var requests = await _apiClient.GetAsync<List<BloodRequestViewModel>>("api/bloodrequests", cancellationToken);
            var hospitals = await _apiClient.GetAsync<List<HospitalViewModel>>("api/hospitals", cancellationToken);
            return View(new DashboardViewModel { Requests = requests, Hospitals = hospitals });
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.Hospitals = await _apiClient.GetAsync<List<HospitalViewModel>>("api/hospitals", cancellationToken);
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            ViewBag.Hospitals = new List<HospitalViewModel>();
        }

        return View(new CreateRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await Create(cancellationToken);
        }

        try
        {
            await _apiClient.PostAsync<BloodRequestViewModel>("api/bloodrequests", model, cancellationToken);
            TempData["Success"] = "Blood request created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await CreateViewWithHospitals(model, cancellationToken);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Matches(int requestId, CancellationToken cancellationToken)
    {
        try
        {
            var matches = await _apiClient.GetAsync<List<MatchViewModel>>($"api/matches/{requestId}", cancellationToken);
            return View(matches);
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new List<MatchViewModel>());
        }
    }

    private async Task<IActionResult> CreateViewWithHospitals(CreateRequestViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.Hospitals = await _apiClient.GetAsync<List<HospitalViewModel>>("api/hospitals", cancellationToken);
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            ViewBag.Hospitals = new List<HospitalViewModel>();
        }

        return View("Create", model);
    }
}
