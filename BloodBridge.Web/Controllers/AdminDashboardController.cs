using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminDashboardController : Controller
{
    private readonly ApiClient _apiClient;

    public AdminDashboardController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? role = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var model = new AdminDashboardViewModel();
        try
        {
            model.Stats = await _apiClient.GetAsync<AdminStatsViewModel>("api/admin/stats", cancellationToken);
            model.Users = await _apiClient.GetAsync<AdminUserPageViewModel>(
                $"api/admin/users?page={Math.Max(page, 1)}&pageSize=20&role={Uri.EscapeDataString(role ?? string.Empty)}&status={Uri.EscapeDataString(status ?? string.Empty)}",
                cancellationToken);
            model.Requests = await _apiClient.GetAsync<List<BloodRequestViewModel>>("api/bloodrequests", cancellationToken);
        }
        catch (ApiException exception)
        {
            ViewBag.Error = exception.Message;
        }

        ViewBag.Role = role;
        ViewBag.Status = status;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserStatus(string id, string status, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.PutAsync<AdminUserViewModel>($"api/admin/users/{Uri.EscapeDataString(id)}", new { Status = status }, cancellationToken);
            TempData["Success"] = "User status updated.";
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
