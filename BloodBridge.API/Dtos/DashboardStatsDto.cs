namespace BloodBridge.API.Dtos;

public sealed class DashboardStatsDto
{
    public int TotalDonors { get; init; }
    public int ActiveBloodRequests { get; init; }
    public int FulfilledRequests { get; init; }
    public int TotalHospitals { get; init; }
    public Dictionary<string, int> RequestsByBloodGroup { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
