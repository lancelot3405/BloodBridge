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
            NextSafeDonationDate = nextSafeDonationDate
        });
    }
}
