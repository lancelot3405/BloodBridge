using BloodBridge.API.Data;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/bloodrequests")]
public class BloodRequestsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;

    public BloodRequestsController(BloodBridgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BloodRequest>>> GetBloodRequests()
    {
        return await _context.BloodRequests.AsNoTracking().ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<BloodRequest>> CreateBloodRequest(BloodRequest bloodRequest)
    {
        var hospitalExists = await _context.Hospitals.AnyAsync(hospital => hospital.Id == bloodRequest.HospitalId);

        if (!hospitalExists)
        {
            return BadRequest(new { message = "The specified HospitalId does not exist." });
        }

        bloodRequest.CreatedAt = DateTime.UtcNow;
        _context.BloodRequests.Add(bloodRequest);
        await _context.SaveChangesAsync();

        return Created($"/api/bloodrequests/{bloodRequest.Id}", bloodRequest);
    }
}
