using System.ComponentModel.DataAnnotations;
using BloodBridge.API.Models;

namespace BloodBridge.API.Dtos;

public sealed class UpdateDonorDto
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; init; } = string.Empty;

    [Required, StringLength(100)]
    [Coordinates]
    public string Location { get; init; } = string.Empty;

    public bool IsAvailable { get; init; }
}
