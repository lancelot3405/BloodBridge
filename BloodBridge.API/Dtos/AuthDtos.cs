using System.ComponentModel.DataAnnotations;
using BloodBridge.API.Models;

namespace BloodBridge.API.Dtos;

public sealed class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = string.Empty;
}

public sealed class RegisterDonorDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(3)]
    public string BloodGroup { get; init; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; init; } = string.Empty;

    [Required, StringLength(100)]
    [Coordinates]
    public string Location { get; init; } = string.Empty;

    public bool IsAvailable { get; init; }
}

public sealed class RegisterHospitalDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string HospitalName { get; init; } = string.Empty;

    [Required, StringLength(250)]
    [Coordinates]
    public string Location { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string ContactInfo { get; init; } = string.Empty;
}

public sealed class RegisterRequesterDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string ContactNumber { get; init; } = string.Empty;
}

public sealed class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Donor = "Donor";
    public const string Requester = "Requester";
    public const string Hospital = "Hospital";

    public static readonly string[] All = [Admin, Donor, Requester, Hospital];
}
