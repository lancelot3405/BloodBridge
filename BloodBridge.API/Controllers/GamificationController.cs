using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/gamification")]
[Authorize(Roles = ApplicationRoles.Donor)]
public sealed class GamificationController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IGamificationService _gamificationService;

    public GamificationController(
        BloodBridgeDbContext context,
        ICurrentUserService currentUser,
        IGamificationService gamificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _gamificationService = gamificationService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<GamificationProfileResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var donor = await _context.Donors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        if (donor is null)
        {
            return NotFound(new { message = "Donor profile not found." });
        }

        var streakProfile = await _gamificationService.UpdateStreakAsync(donor.Id, cancellationToken);
        var profile = await _gamificationService.GetProfileAsync(donor.Id, cancellationToken);
        var nextSafeDonationDate = DonorMedicalEligibility.GetNextSafeDonationDate(donor.LastDonationDate);

        return Ok(new GamificationProfileResponseDto
        {
            DonorId = donor.Id,
            ImpactScore = profile.ImpactScore,
            TierRank = profile.TierRank,
            NextRankTarget = GamificationRules.GetNextTier(profile.ImpactScore).Target,
            NextTierRank = GamificationRules.GetNextTier(profile.ImpactScore).Tier,
            BadgesEarned = profile.BadgesEarned,
            IsMedicallyEligible = DonorMedicalEligibility.IsEligible(donor.LastDonationDate, DateTime.UtcNow),
            LastDonationDate = donor.LastDonationDate,
            NextSafeDonationDate = nextSafeDonationDate,
            CurrentStreak = streakProfile.CurrentStreak,
            HighestStreak = streakProfile.HighestStreak,
            LastActiveDate = streakProfile.LastActiveDate
        });
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<ImpactActivityLogDto>>> GetHistory(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var donor = await _context.Donors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        if (donor is null)
        {
            return NotFound(new { message = "Donor profile not found." });
        }

        return Ok(await _gamificationService.GetActivityHistoryAsync(donor.Id, limit, cancellationToken));
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<IReadOnlyList<SeasonalLeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var selectedYear = year ?? now.Year;
        var selectedMonth = month ?? now.Month;
        if (selectedYear is < 1 or > 9999 || selectedMonth is < 1 or > 12)
        {
            return BadRequest(new { message = "A valid year and month are required." });
        }

        return Ok(await _gamificationService.GetSeasonalLeaderboardAsync(
            selectedYear,
            selectedMonth,
            cancellationToken));
    }
}
