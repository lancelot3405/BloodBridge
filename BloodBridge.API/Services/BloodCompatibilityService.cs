using BloodBridge.API.Models;

namespace BloodBridge.API.Services;

public class BloodCompatibilityService
{
    private readonly GeographicService _geographicService;

    public BloodCompatibilityService(GeographicService? geographicService = null)
    {
        _geographicService = geographicService ?? new GeographicService();
    }
    private static readonly Dictionary<string, string[]> CompatibleDonorGroups =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["A+"] = ["A+", "A-", "O+", "O-"],
            ["A-"] = ["A-", "O-"],
            ["B+"] = ["B+", "B-", "O+", "O-"],
            ["B-"] = ["B-", "O-"],
            ["AB+"] = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"],
            ["AB-"] = ["A-", "B-", "AB-", "O-"],
            ["O+"] = ["O+", "O-"],
            ["O-"] = ["O-"]
        };

    public bool IsCompatible(string requestedBloodGroup, string donorBloodGroup)
    {
        return CompatibleDonorGroups.TryGetValue(requestedBloodGroup, out var donorGroups)
            && donorGroups.Contains(donorBloodGroup, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsKnownBloodGroup(string bloodGroup)
    {
        return IsKnownBloodGroupValue(bloodGroup);
    }

    public static bool IsKnownBloodGroupValue(string? bloodGroup)
    {
        return !string.IsNullOrWhiteSpace(bloodGroup)
            && CompatibleDonorGroups.ContainsKey(bloodGroup.Trim());
    }

    public IEnumerable<Donor> FindCompatibleDonors(
        string requestedBloodGroup,
        IEnumerable<Donor> donors,
        string? hospitalLocation = null)
    {
        return donors
            .Where(donor => donor.IsAvailable && IsCompatible(requestedBloodGroup, donor.BloodGroup))
            .OrderBy(donor => CalculateDistanceKm(donor.Location, hospitalLocation) ?? double.MaxValue)
            .ThenBy(donor => IsSameLocation(donor.Location, hospitalLocation) ? 0 : 1)
            .ThenBy(donor => donor.Location)
            .ThenBy(donor => donor.Name);
    }

    public double? CalculateDistanceKm(string? firstLocation, string? secondLocation) =>
        _geographicService.CalculateDistanceKm(firstLocation, secondLocation);

    public bool TryParseCoordinates(string? location, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;
        if (!_geographicService.TryParseCoordinates(location, out var coordinate))
        {
            return false;
        }

        latitude = coordinate.Latitude;
        longitude = coordinate.Longitude;
        return true;
    }

    public bool IsSameLocation(string donorLocation, string? hospitalLocation)
    {
        var distance = CalculateDistanceKm(donorLocation, hospitalLocation);
        return distance.HasValue && distance.Value <= 0.01;
    }
}
