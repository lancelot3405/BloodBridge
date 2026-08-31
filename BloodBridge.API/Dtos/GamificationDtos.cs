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
    public int CurrentStreak { get; init; }
    public int HighestStreak { get; init; }
    public DateTime LastActiveDate { get; init; }
}

public sealed class ImpactActivityLogDto
{
    public int Id { get; init; }
    public string ActivityName { get; init; } = string.Empty;
    public int PointsEarned { get; init; }
    public DateTime EarnedAt { get; init; }
}

public sealed class SeasonalLeaderboardEntryDto
{
    public int Rank { get; init; }
    public int DonorId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int SeasonalScore { get; init; }
    public string TierRank { get; init; } = string.Empty;
}
