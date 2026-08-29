using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[Authorize]
public sealed class NotificationsController : Controller
{
    private readonly ApiClient _apiClient;

    public NotificationsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("notifications/unread")]
    public async Task<IActionResult> Unread(CancellationToken cancellationToken)
    {
        try
        {
            return Json(await _apiClient.GetAsync<List<NotificationViewModel>>("api/notifications", cancellationToken));
        }
        catch (ApiException exception)
        {
            return StatusCode(exception.StatusCode, new { message = exception.Message });
        }
    }

    [HttpPost("notifications/{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.PutNoContentAsync($"api/notifications/{id}/read", new { }, cancellationToken);
            return NoContent();
        }
        catch (ApiException exception)
        {
            return StatusCode(exception.StatusCode, new { message = exception.Message });
        }
    }
}
