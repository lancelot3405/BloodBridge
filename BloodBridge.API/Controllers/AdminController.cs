using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class AdminController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(BloodBridgeDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("stats")]
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var activeStatuses = new[] { "pending", "matched" };
        var counts = await _context.BloodRequests
            .AsNoTracking()
            .Where(request => new[] { "O+", "A+", "B+", "AB+" }.Contains(request.BloodGroup))
            .GroupBy(request => request.BloodGroup)
            .Select(group => new { BloodGroup = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var requestsByBloodGroup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["O+"] = 0,
            ["A+"] = 0,
            ["B+"] = 0,
            ["AB+"] = 0
        };
        foreach (var item in counts)
        {
            requestsByBloodGroup[item.BloodGroup] = item.Count;
        }

        return Ok(new DashboardStatsDto
        {
            TotalDonors = await _context.Donors.CountAsync(cancellationToken),
            ActiveBloodRequests = await _context.BloodRequests.CountAsync(
                request => activeStatuses.Contains(request.Status.ToLower()), cancellationToken),
            FulfilledRequests = await _context.BloodRequests.CountAsync(
                request => request.Status.ToLower() == "fulfilled", cancellationToken),
            TotalHospitals = await _context.Hospitals.CountAsync(cancellationToken),
            RequestsByBloodGroup = requestsByBloodGroup
        });
    }

    [HttpGet("users")]
    public async Task<ActionResult<AdminUserPageDto>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var now = DateTimeOffset.UtcNow;
        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            usersQuery = status.Trim().ToLowerInvariant() switch
            {
                "active" or "verified" => usersQuery.Where(user => user.IsActive && (!user.LockoutEnd.HasValue || user.LockoutEnd <= now)),
                "suspended" => usersQuery.Where(user => user.IsActive && user.LockoutEnd > now),
                "deactivated" or "inactive" => usersQuery.Where(user => !user.IsActive),
                "unverified" => usersQuery.Where(user => !user.EmailConfirmed),
                _ => usersQuery
            };
        }

        var users = await usersQuery.OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var filtered = new List<AdminUserListItemDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "None";
            if (!string.IsNullOrWhiteSpace(role)
                && !string.Equals(userRole, role.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            filtered.Add(ToUserDto(user, userRole, now));
        }

        return Ok(new AdminUserPageDto
        {
            Items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = filtered.Count
        });
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<AdminUserListItemDto>> UpdateUserStatus(
        string id,
        UpdateUserStatusDto input,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        switch (input.Status.Trim().ToLowerInvariant())
        {
            case "verify":
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.LockoutEnd = null;
                break;
            case "activate":
                user.IsActive = true;
                user.LockoutEnd = null;
                break;
            case "suspend":
                user.IsActive = true;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                break;
            case "deactivate":
                user.IsActive = false;
                user.LockoutEnd = null;
                break;
            default:
                return BadRequest(new { message = "Status must be verify, activate, suspend, or deactivate." });
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(error => error.Description)) });
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ToUserDto(user, roles.FirstOrDefault() ?? "None", DateTimeOffset.UtcNow));
    }

    private static AdminUserListItemDto ToUserDto(ApplicationUser user, string role, DateTimeOffset now) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        Role = role,
        EmailConfirmed = user.EmailConfirmed,
        IsActive = user.IsActive && (!user.LockoutEnd.HasValue || user.LockoutEnd <= now),
        Status = !user.IsActive ? "Deactivated" : user.LockoutEnd > now ? "Suspended" : user.EmailConfirmed ? "Verified" : "Unverified"
    };
}
