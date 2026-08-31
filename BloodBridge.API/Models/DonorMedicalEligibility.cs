namespace BloodBridge.API.Models;

public static class DonorMedicalEligibility
{
    public const int MinimumDonationIntervalDays = 90;
    public const string LockMessage = "Medical Eligibility Lock: You must wait 90 days between donations";

    public static DateTime? GetNextSafeDonationDate(DateTime? lastDonationDate) =>
        lastDonationDate?.AddDays(MinimumDonationIntervalDays);

    public static bool IsEligible(DateTime? lastDonationDate, DateTime utcNow) =>
        !GetNextSafeDonationDate(lastDonationDate).HasValue
        || GetNextSafeDonationDate(lastDonationDate) <= utcNow;

    public static string? GetEligibilityError(DateTime? lastDonationDate, DateTime utcNow) =>
        IsEligible(lastDonationDate, utcNow) ? null : LockMessage;
}
