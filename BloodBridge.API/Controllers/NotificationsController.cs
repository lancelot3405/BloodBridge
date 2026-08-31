using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(NotificationService notificationService, ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetUnread(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        return await _notificationService.GetUnreadAsync(_currentUser.UserId, cancellationToken);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        return await _notificationService.MarkReadAsync(id, _currentUser.UserId, cancellationToken)
            ? NoContent()
            : NotFound(new { message = "Notification not found." });
    }
}
