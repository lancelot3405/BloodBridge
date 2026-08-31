using BloodBridge.API.Models;

namespace BloodBridge.API.Dtos;

public sealed class DonorProfileResponseDto
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string BloodGroup { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public DateTime? LastDonationDate { get; init; }
}

public sealed class UpdateDonorProfileDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.Phone,
     System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Phone { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100)]
    [Coordinates]
    public string Location { get; init; } = string.Empty;

    public bool IsAvailable { get; init; }
}

public sealed class HospitalProfileResponseDto
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string HospitalName { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string ContactInfo { get; init; } = string.Empty;
}

public sealed class UpdateHospitalProfileDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150)]
    public string HospitalName { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(250)]
    [Coordinates]
    public string Location { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(20)]
    public string ContactInfo { get; init; } = string.Empty;
}
