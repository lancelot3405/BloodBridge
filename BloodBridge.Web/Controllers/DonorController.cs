using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[Authorize(Roles = "Donor")]
public sealed class DonorController : Controller
{
    private readonly ApiClient _apiClient;

    public DonorController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _apiClient.GetAsync<DonorViewModel>("api/donors/me", cancellationToken);
            var requests = await _apiClient.GetAsync<List<BloodRequestViewModel>>("api/bloodrequests", cancellationToken);
            GamificationProfileViewModel? gamification = null;
            try
            {
                gamification = await _apiClient.GetAsync<GamificationProfileViewModel>(
                    "api/gamification/me",
                    cancellationToken);
            }
            catch (ApiException exception)
            {
                ViewBag.GamificationError = exception.Message;
            }

            requests = requests
                .Where(request => request.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)
                    && CanDonate(profile.BloodGroup, request.BloodGroup))
                .ToList();
            return View(new DonorDashboardViewModel
            {
                Profile = profile,
                Gamification = gamification,
                Requests = requests
            });
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new DonorDashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _apiClient.GetAsync<DonorViewModel>("api/donors/me", cancellationToken));
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
            return View(new DonorViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateDonorProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.PutAsync<DonorViewModel>("api/donors/me", model, cancellationToken);
            TempData["Success"] = "Donor profile updated.";
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
    public async Task<IActionResult> Accept(int requestId, int donorId, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.PutAsync<BloodRequestViewModel>(
                $"api/bloodrequests/{requestId}/status",
                new { Status = "DONOR ACCEPTED", DonorId = donorId },
                cancellationToken);
            TempData["Success"] = "Request accepted. Please coordinate with the hospital.";
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Impact(CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var profile = await _apiClient.GetAsync<GamificationProfileViewModel>(
                "api/gamification/me",
                cancellationToken);
            var history = await _apiClient.GetAsync<List<ImpactActivityLogViewModel>>(
                "api/gamification/history?limit=50",
                cancellationToken);
            var leaderboard = await _apiClient.GetAsync<List<SeasonalLeaderboardEntryViewModel>>(
                $"api/gamification/leaderboard?year={now.Year}&month={now.Month}",
                cancellationToken);

            return View("~/Views/Gamification/Dashboard.cshtml", new GamificationDashboardViewModel
            {
                Profile = profile,
                History = history,
                Leaderboard = leaderboard,
                SeasonLabel = now.ToString("MMMM yyyy")
            });
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private static bool CanDonate(string donorBloodGroup, string requestedBloodGroup) =>
        (requestedBloodGroup.ToUpperInvariant(), donorBloodGroup.ToUpperInvariant()) switch
        {
            ("A+", "A+" or "A-" or "O+" or "O-") => true,
            ("A-", "A-" or "O-") => true,
            ("B+", "B+" or "B-" or "O+" or "O-") => true,
            ("B-", "B-" or "O-") => true,
            ("AB+", _) => true,
            ("AB-", "A-" or "B-" or "AB-" or "O-") => true,
            ("O+", "O+" or "O-") => true,
            ("O-", "O-") => true,
            _ => false
        };
}
