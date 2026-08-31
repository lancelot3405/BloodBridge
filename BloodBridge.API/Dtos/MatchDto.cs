namespace BloodBridge.API.Dtos;

public sealed class MatchDto
{
    public int DonorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BloodGroup { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public double? DistanceKm { get; init; }

    // True when the donor and hospital coordinates are within 10 metres.
    public bool SameLocationAsHospital { get; init; }
}
