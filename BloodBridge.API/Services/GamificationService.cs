using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Services;

public interface IGamificationService
{
    Task<GamificationProfile> AwardActivityXpAsync(
        int donorId,
        string activityType,
        int? bloodRequestId = null,
        CancellationToken cancellationToken = default);

    Task<GamificationProfile> GetProfileAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    Task<GamificationProfile> UpdateStreakAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpactActivityLogDto>> GetActivityHistoryAsync(
        int donorId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeasonalLeaderboardEntryDto>> GetSeasonalLeaderboardAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default);
}

public sealed class GamificationService : IGamificationService
{
    private readonly BloodBridgeDbContext _context;

    public GamificationService(BloodBridgeDbContext context)
    {
        _context = context;
    }

    public async Task<GamificationProfile> AwardActivityXpAsync(
        int donorId,
        string activityType,
        int? bloodRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedActivity = activityType.Trim().ToUpperInvariant();
        if (!GamificationRules.ActivityPoints.TryGetValue(normalizedActivity, out var points))
        {
            throw new ArgumentException($"Unknown gamification activity: {activityType}", nameof(activityType));
        }

        var donor = await _context.Donors
            .FirstOrDefaultAsync(item => item.Id == donorId, cancellationToken);
        if (donor is null)
        {
            throw new InvalidOperationException("The specified donor does not exist.");
        }

        var profile = await GetOrCreateProfileAsync(donorId, cancellationToken);
        var activityKey = BuildActivityKey(normalizedActivity, bloodRequestId);
        var activityExists = await _context.GamificationActivities
            .AnyAsync(item => item.DonorId == donorId && item.ActivityKey == activityKey, cancellationToken)
            || normalizedActivity == GamificationActivityTypes.CompleteProfile
                && profile.ProfileCompletedXPGranted;

        if (!activityExists)
        {
            _context.GamificationActivities.Add(new GamificationActivity
            {
                DonorId = donorId,
                ActivityType = normalizedActivity,
                ActivityKey = activityKey,
                BloodRequestId = bloodRequestId,
                PointsAwarded = points,
                AwardedAt = DateTime.UtcNow
            });

            _context.ImpactActivityLogs.Add(new ImpactActivityLog
            {
                DonorId = donorId,
                ActivityName = GetActivityName(normalizedActivity),
                PointsEarned = points,
                EarnedAt = DateTime.UtcNow
            });

            profile.ImpactScore += points;
            if (normalizedActivity == GamificationActivityTypes.CompleteProfile)
            {
                profile.ProfileCompletedXPGranted = true;
            }
        }

        profile.TierRank = GamificationRules.GetTier(profile.ImpactScore);
        if (normalizedActivity == GamificationActivityTypes.UrgentRequest)
        {
            AddBadge(profile, GamificationBadges.EmergencyHero);
        }
        await UpdateBadgesAsync(profile, donor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<GamificationProfile> UpdateStreakAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateProfileAsync(donorId, cancellationToken);
        var today = DateTime.UtcNow.Date;
        var lastActiveDate = profile.LastActiveDate.Date;

        if (lastActiveDate == today)
        {
            return profile;
        }

        profile.CurrentStreak = lastActiveDate == today.AddDays(-1)
            ? profile.CurrentStreak + 1
            : 1;
        profile.HighestStreak = Math.Max(profile.HighestStreak, profile.CurrentStreak);
        profile.LastActiveDate = today;

        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<IReadOnlyList<ImpactActivityLogDto>> GetActivityHistoryAsync(
        int donorId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        return await _context.ImpactActivityLogs
            .AsNoTracking()
            .Where(log => log.DonorId == donorId)
            .OrderByDescending(log => log.EarnedAt)
            .Take(limit)
            .Select(log => new ImpactActivityLogDto
            {
                Id = log.Id,
                ActivityName = log.ActivityName,
                PointsEarned = log.PointsEarned,
                EarnedAt = log.EarnedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SeasonalLeaderboardEntryDto>> GetSeasonalLeaderboardAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year is < 1 or > 9999 || month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "A valid year and month are required.");
        }

        var start = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var totals = await _context.ImpactActivityLogs
            .AsNoTracking()
            .Where(log => log.EarnedAt >= start && log.EarnedAt < end)
            .GroupBy(log => log.DonorId)
            .Select(group => new { DonorId = group.Key, SeasonalScore = group.Sum(log => log.PointsEarned) })
            .OrderByDescending(item => item.SeasonalScore)
            .ThenBy(item => item.DonorId)
            .Take(10)
            .ToListAsync(cancellationToken);

        var donorIds = totals.Select(item => item.DonorId).ToArray();
        var donors = await _context.Donors
            .AsNoTracking()
            .Where(donor => donorIds.Contains(donor.Id))
            .ToDictionaryAsync(donor => donor.Id, cancellationToken);

        return totals
            .Where(item => donors.ContainsKey(item.DonorId))
            .Select((item, index) => new SeasonalLeaderboardEntryDto
            {
                Rank = index + 1,
                DonorId = item.DonorId,
                DisplayName = MaskName(donors[item.DonorId].Name),
                SeasonalScore = item.SeasonalScore,
                TierRank = GamificationRules.GetTier(item.SeasonalScore)
            })
            .ToList();
    }

    public async Task<GamificationProfile> GetProfileAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        var donorExists = await _context.Donors
            .AnyAsync(item => item.Id == donorId, cancellationToken);
        if (!donorExists)
        {
            throw new InvalidOperationException("The specified donor does not exist.");
        }

        var profile = await _context.GamificationProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.DonorId == donorId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        profile = new GamificationProfile { DonorId = donorId };
        _context.GamificationProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private async Task<GamificationProfile> GetOrCreateProfileAsync(
        int donorId,
        CancellationToken cancellationToken)
    {
        var profile = await _context.GamificationProfiles
            .FirstOrDefaultAsync(item => item.DonorId == donorId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        profile = new GamificationProfile { DonorId = donorId };
        _context.GamificationProfiles.Add(profile);
        return profile;
    }

    private async Task UpdateBadgesAsync(
        GamificationProfile profile,
        Donor donor,
        CancellationToken cancellationToken)
    {
        profile.BadgesEarned ??= [];

        if (IsProfileComplete(donor))
        {
            AddBadge(profile, GamificationBadges.ProfileComplete);
        }

        var donationCount = await _context.Donations
            .CountAsync(item => item.DonorId == donor.Id, cancellationToken);
        if (donationCount >= 1)
        {
            AddBadge(profile, GamificationBadges.FirstDrop);
        }

        var hasUrgentAcceptance = await _context.GamificationActivities
            .AnyAsync(item => item.DonorId == donor.Id
                && item.ActivityType == GamificationActivityTypes.UrgentRequest,
                cancellationToken);
        if (hasUrgentAcceptance)
        {
            AddBadge(profile, GamificationBadges.EmergencyHero);
        }

        var respondedMatches = await _context.DonorMatches
            .CountAsync(item => item.DonorId == donor.Id
                && (item.Status == "Accepted" || item.Status == "Declined"), cancellationToken);
        var acceptedMatches = await _context.DonorMatches
            .CountAsync(item => item.DonorId == donor.Id && item.Status == "Accepted", cancellationToken);
        if (respondedMatches >= 3 && acceptedMatches * 100 >= respondedMatches * 80)
        {
            AddBadge(profile, GamificationBadges.ReliableDonor);
        }
    }

    private static bool IsProfileComplete(Donor donor) =>
        !string.IsNullOrWhiteSpace(donor.Name)
        && !string.IsNullOrWhiteSpace(donor.BloodGroup)
        && !string.IsNullOrWhiteSpace(donor.Phone)
        && !string.IsNullOrWhiteSpace(donor.Location);

    private static void AddBadge(GamificationProfile profile, string badge)
    {
        if (!profile.BadgesEarned.Contains(badge, StringComparer.Ordinal))
        {
            profile.BadgesEarned.Add(badge);
        }
    }

    private static string BuildActivityKey(string activityType, int? bloodRequestId) =>
        bloodRequestId.HasValue
            ? $"{activityType}:{bloodRequestId.Value}"
            : activityType;

    private static string GetActivityName(string activityType) => activityType switch
    {
        GamificationActivityTypes.CompleteProfile => "Profile Completed",
        GamificationActivityTypes.VerifyDonorInformation => "Verified Donor Information",
        GamificationActivityTypes.SetAvailability => "Availability Updated",
        GamificationActivityTypes.RespondToRequest => "Accepted Request",
        GamificationActivityTypes.UrgentRequest => "Responded to Urgent Request",
        GamificationActivityTypes.SuccessfulDonation => "Successful Verified Donation",
        _ => activityType
    };

    private static string MaskName(string? name)
    {
        var parts = name?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();
        return parts.Length switch
        {
            0 => "Community Donor",
            1 => parts[0],
            _ => $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}."
        };
    }
}
