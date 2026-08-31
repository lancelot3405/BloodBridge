using System.ComponentModel.DataAnnotations;

namespace BloodBridge.API.Dtos;

public sealed class CreateDonationDto
{
    [Range(1, int.MaxValue)]
    public int DonorId { get; init; }

    [Range(1, int.MaxValue)]
    public int BloodRequestId { get; init; }

    // Optional for convenience; the API records UTC now when omitted.
    public DateTime? DonationDate { get; init; }
}
