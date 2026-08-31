using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly BloodCompatibilityService _compatibilityService;
    private readonly GeographicService _geographicService;
    private readonly IDonorRankingService _rankingService;

    public MatchesController(
        BloodBridgeDbContext context,
        BloodCompatibilityService compatibilityService,
        GeographicService geographicService,
        IDonorRankingService rankingService)
    {
        _context = context;
        _compatibilityService = compatibilityService;
        _geographicService = geographicService;
        _rankingService = rankingService;
    }

    [HttpGet("{requestId}")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches(
        int requestId,
        CancellationToken cancellationToken)
    {
        var request = await _context.BloodRequests
            .AsNoTracking()
            .Include(item => item.Hospital)
            .FirstOrDefaultAsync(item => item.Id == requestId, cancellationToken);

        if (request == null)
        {
            return NotFound(new { message = "Blood request not found." });
        }

        if (!_compatibilityService.IsKnownBloodGroup(request.BloodGroup))
        {
            return BadRequest(new { message = "The blood request has an invalid BloodGroup." });
        }

        var donors = await _context.Donors
            .AsNoTracking()
            .Where(donor => donor.IsAvailable)
            .ToListAsync(cancellationToken);

        var eligibleDonors = _compatibilityService
            .FindCompatibleDonors(request.BloodGroup, donors, request.Hospital?.Location)
            .ToList();
        foreach (var donor in eligibleDonors)
        {
            donor.DistanceKmForRanking = _geographicService.CalculateDistanceKm(
                donor.Location,
                request.Hospital?.Location);
        }

        var matches = _rankingService
            .RankDonors(eligibleDonors)
            .Select(donor => new MatchDto
            {
                DonorId = donor.Id,
                Name = donor.Name,
                BloodGroup = donor.BloodGroup,
                Location = donor.Location,
                IsAvailable = donor.IsAvailable,
                DistanceKm = donor.DistanceKmForRanking,
                SameLocationAsHospital = _compatibilityService.IsSameLocation(
                    donor.Location,
                    request.Hospital?.Location)
            })
            .ToList();

        return matches;
    }
}
