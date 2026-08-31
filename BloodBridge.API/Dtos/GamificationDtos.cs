namespace BloodBridge.API.Dtos;

public sealed class GamificationProfileResponseDto
{
    public int DonorId { get; init; }
    public int ImpactScore { get; init; }
    public string TierRank { get; init; } = string.Empty;
    public int? NextRankTarget { get; init; }
    public string? NextTierRank { get; init; }
    public IReadOnlyCollection<string> BadgesEarned { get; init; } = Array.Empty<string>();
    public bool IsMedicallyEligible { get; init; }
    public DateTime? LastDonationDate { get; init; }
    public DateTime? NextSafeDonationDate { get; init; }
}
