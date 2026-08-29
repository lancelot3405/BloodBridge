using System.ComponentModel.DataAnnotations;

namespace BloodBridge.Web.ViewModels;

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class RegisterViewModel
{
    [Required]
    public string Role { get; set; } = "Donor";

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = "O+";
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string HospitalName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
}
