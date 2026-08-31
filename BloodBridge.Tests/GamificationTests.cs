using BloodBridge.API.Models;

namespace BloodBridge.Tests;

public sealed class GamificationTests
{
    [Fact]
    public void DonorWithRecentDonationIsRejectedByNinetyDayEligibilityLock()
    {
        var now = DateTime.UtcNow;
        var lastDonationDate = now.AddDays(-30);

        var error = DonorMedicalEligibility.GetEligibilityError(lastDonationDate, now);

        Assert.Equal(DonorMedicalEligibility.LockMessage, error);
        Assert.False(DonorMedicalEligibility.IsEligible(lastDonationDate, now));
    }
}
