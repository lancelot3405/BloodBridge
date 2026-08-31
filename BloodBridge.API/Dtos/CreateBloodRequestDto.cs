using System.ComponentModel.DataAnnotations;

namespace BloodBridge.API.Dtos;

public sealed class CreateBloodRequestDto
{
    [Range(1, int.MaxValue)]
    public int HospitalId { get; init; }

    [Required, StringLength(3)]
    public string BloodGroup { get; init; } = string.Empty;

    [Range(1, 100)]
    public int UnitsRequired { get; init; }

    [Required, StringLength(20)]
    public string Urgency { get; init; } = string.Empty;

    [Required]
    public DateTime? RequiredDate { get; init; }
}
