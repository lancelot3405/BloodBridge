namespace BloodBridge.API.Dtos;

public sealed class BloodRequestStatusResponseDto
{
    public int Id { get; init; }
    public int HospitalId { get; init; }
    public string BloodGroup { get; init; } = string.Empty;
    public int UnitsRequired { get; init; }
    public string Urgency { get; init; } = string.Empty;
    public DateTime RequiredDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int? AcceptedDonorId { get; init; }
}
