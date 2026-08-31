using BloodBridge.API.Data;
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
}
