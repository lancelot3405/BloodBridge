using BloodBridge.API.Models;
using BloodBridge.API.Services;

namespace BloodBridge.Tests;

public sealed class BloodCompatibilityTests
{
    private readonly BloodCompatibilityService _service = new();

    [Fact]
    public void OPositiveRequestMatchesOnlyOPositiveAndONegativeDonors()
    {
        Assert.True(_service.IsCompatible("O+", "O+"));
        Assert.True(_service.IsCompatible("O+", "O-"));
        Assert.False(_service.IsCompatible("O+", "A+"));
        Assert.False(_service.IsCompatible("O+", "B+"));
        Assert.False(_service.IsCompatible("O+", "AB+"));
    }

    [Fact]
    public void ABPositiveRequestAcceptsAllRedCellDonorTypes()
    {
        var bloodGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        foreach (var bloodGroup in bloodGroups)
        {
            Assert.True(_service.IsCompatible("AB+", bloodGroup), bloodGroup);
        }
    }

    [Fact]
    public void FindCompatibleDonorsExcludesUnavailableAndIncompatibleDonors()
    {
        var donors = new[]
        {
            new Donor { Id = 1, Name = "O positive", BloodGroup = "O+", Location = "23.25,77.41", IsAvailable = true },
            new Donor { Id = 2, Name = "O negative", BloodGroup = "O-", Location = "23.26,77.42", IsAvailable = true },
            new Donor { Id = 3, Name = "A positive", BloodGroup = "A+", Location = "23.25,77.41", IsAvailable = true },
            new Donor { Id = 4, Name = "Unavailable O", BloodGroup = "O+", Location = "23.25,77.41", IsAvailable = false }
        };

        var result = _service.FindCompatibleDonors("O+", donors, "23.25,77.41").Select(donor => donor.Id).ToArray();

        Assert.Equal(new[] { 1, 2 }, result);
    }
}
