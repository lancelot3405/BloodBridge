using System.ComponentModel.DataAnnotations;

namespace BloodBridge.API.Dtos;

public sealed class UpdateBloodRequestStatusDto
{
    [Required]
    public string Status { get; init; } = string.Empty;

    // Required when moving to DONOR ACCEPTED.
    public int? DonorId { get; init; }

    // Optional; the server uses UTC now when recording a completed donation.
    public DateTime? DonationDate { get; init; }
}
