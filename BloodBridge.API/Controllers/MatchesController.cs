using BloodBridge.API.Data;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly BloodCompatibilityService _compatibilityService;

    public MatchesController(BloodBridgeDbContext context, BloodCompatibilityService compatibilityService)
    {
        _context = context;
        _compatibilityService = compatibilityService;
    }

    [HttpGet("{requestId}")]
    public async Task<ActionResult<IEnumerable<Donor>>> GetMatches(int requestId)
    {
        var request = await _context.BloodRequests.AsNoTracking().FirstOrDefaultAsync(item => item.Id == requestId);

        if (request == null)
        {
            return NotFound(new { message = "Blood request not found." });
        }

        var donors = await _context.Donors.AsNoTracking().ToListAsync();
        var matches = _compatibilityService.FindCompatibleDonors(request.BloodGroup, donors).ToList();

        return matches;
    }
}
