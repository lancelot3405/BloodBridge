using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public sealed class GamificationProfile
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    public int ImpactScore { get; set; }

    [Required, StringLength(50)]
    public string TierRank { get; set; } = GamificationRules.NewDonorTier;

    // Persisted as JSON by BloodBridgeDbContext.
    public List<string> BadgesEarned { get; set; } = [];

    public bool ProfileCompletedXPGranted { get; set; }

    public int CurrentStreak { get; set; }

    public int HighestStreak { get; set; }

    public DateTime LastActiveDate { get; set; }

    [JsonIgnore]
    public Donor Donor { get; set; } = null!;
}

public sealed class GamificationActivity
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required, StringLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ActivityKey { get; set; } = string.Empty;

    public int? BloodRequestId { get; set; }

    public int PointsAwarded { get; set; }

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Donor Donor { get; set; } = null!;
}

public static class GamificationActivityTypes
{
    public const string CompleteProfile = "COMPLETE_PROFILE";
    public const string VerifyDonorInformation = "VERIFY_DONOR_INFORMATION";
    public const string SetAvailability = "SET_AVAILABILITY";
    public const string RespondToRequest = "RESPOND_TO_REQUEST";
    public const string UrgentRequest = "URGENT_REQUEST";
    public const string SuccessfulDonation = "SUCCESSFUL_DONATION";
}

public static class GamificationBadges
{
    public const string FirstDrop = "First Drop";
    public const string EmergencyHero = "Emergency Hero";
    public const string ReliableDonor = "Reliable Donor";
    public const string ProfileComplete = "Profile Complete";
}

public static class GamificationRules
{
    public const string NewDonorTier = "New Donor";
    public const string ActiveDonorTier = "Active Donor";
    public const string BronzeLifesaverTier = "Bronze Lifesaver";
    public const string SilverLifesaverTier = "Silver Lifesaver";
    public const string GoldLifesaverTier = "Gold Lifesaver";
    public const string PlatinumGuardianTier = "Platinum Guardian";
    public const string BloodHeroTier = "Blood Hero";

    private static readonly (int Threshold, string Name)[] Tiers =
    [
        (0, NewDonorTier),
        (50, ActiveDonorTier),
        (150, BronzeLifesaverTier),
        (300, SilverLifesaverTier),
        (500, GoldLifesaverTier),
        (750, PlatinumGuardianTier),
        (1000, BloodHeroTier)
    ];

    public static readonly IReadOnlyDictionary<string, int> ActivityPoints =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [GamificationActivityTypes.CompleteProfile] = 20,
            [GamificationActivityTypes.VerifyDonorInformation] = 30,
            [GamificationActivityTypes.SetAvailability] = 5,
            [GamificationActivityTypes.RespondToRequest] = 15,
            [GamificationActivityTypes.UrgentRequest] = 50,
            [GamificationActivityTypes.SuccessfulDonation] = 100
        };

    public static string GetTier(int score) =>
        Tiers.Last(tier => score >= tier.Threshold).Name;

    public static (int? Target, string? Tier) GetNextTier(int score)
    {
        var next = Tiers.FirstOrDefault(tier => score < tier.Threshold);
        return next == default ? (null, null) : (next.Threshold, next.Name);
    }
}
