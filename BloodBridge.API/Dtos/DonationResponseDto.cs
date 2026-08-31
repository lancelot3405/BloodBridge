namespace BloodBridge.API.Dtos;

public sealed class DonationResponseDto
{
    public int Id { get; init; }
    public int DonorId { get; init; }
    public int BloodRequestId { get; init; }
    public int HospitalId { get; init; }
    public string BloodGroup { get; init; } = string.Empty;
    public DateTime DonationDate { get; init; }
    public string Status { get; init; } = string.Empty;
}
