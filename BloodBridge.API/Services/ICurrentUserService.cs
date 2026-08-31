using System.Security.Claims;

namespace BloodBridge.API.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll("role").Select(claim => claim.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
