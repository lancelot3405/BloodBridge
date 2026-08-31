using System.ComponentModel.DataAnnotations;

namespace BloodBridge.API.Dtos;

public sealed class AdminUserListItemDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class AdminUserPageDto
{
    public IReadOnlyList<AdminUserListItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class UpdateUserStatusDto
{
    [Required]
    public string Status { get; init; } = string.Empty;
}
