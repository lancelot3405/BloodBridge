using BloodBridge.API.Models;

namespace BloodBridge.API.Services;

public class BloodCompatibilityService
{
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

    public IEnumerable<Donor> FindCompatibleDonors(string requestedBloodGroup, IEnumerable<Donor> donors)
    {
        return donors.Where(donor => donor.IsAvailable && IsCompatible(requestedBloodGroup, donor.BloodGroup));
    }
}
