namespace BloodBridge.API.Models;

public static class BloodRequestStatuses
{
    public const string Pending = "PENDING";
    public const string Matched = "MATCHED";
    public const string DonorAccepted = "DONOR ACCEPTED";
    public const string DonationCompleted = "DONATION COMPLETED";
    public const string Fulfilled = "FULFILLED";

    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        return normalized is Pending
            or Matched
            or DonorAccepted
            or DonationCompleted
            or Fulfilled;
    }

    public static string? GetNext(string currentStatus)
    {
        return currentStatus switch
        {
            Pending => Matched,
            Matched => DonorAccepted,
            DonorAccepted => DonationCompleted,
            DonationCompleted => Fulfilled,
            _ => null
        };
    }
}
