using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/hospitals")]
[Authorize]
public class HospitalsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public HospitalsController(BloodBridgeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hospital>>> GetHospitals()
    {
        return await _context.Hospitals.AsNoTracking().ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<Hospital>> CreateHospital(Hospital hospital)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        if (await _context.Hospitals.AnyAsync(item => item.UserId == _currentUser.UserId))
        {
            return Conflict(new { message = "The authenticated hospital already has a profile." });
        }

        hospital.UserId = _currentUser.UserId;
        _context.Hospitals.Add(hospital);
        await _context.SaveChangesAsync();

        return Created($"/api/hospitals/{hospital.Id}", hospital);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<HospitalProfileResponseDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var hospital = await _context.Hospitals
            .AsNoTracking()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        return hospital is null ? NotFound(new { message = "Hospital profile not found." }) : Ok(ToProfileResponse(hospital));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<HospitalProfileResponseDto>> UpdateMyProfile(
        UpdateHospitalProfileDto input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var hospital = await _context.Hospitals
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        if (hospital is null)
        {
            return NotFound(new { message = "Hospital profile not found." });
        }

        hospital.Name = input.HospitalName.Trim();
        hospital.Location = input.Location.Trim();
        hospital.Phone = input.ContactInfo.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToProfileResponse(hospital));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Hospital>> UpdateHospital(int id, Hospital input)
    {
        var hospital = await _context.Hospitals.FirstOrDefaultAsync(item => item.Id == id);
        if (hospital is null)
        {
            return NotFound(new { message = "Hospital not found." });
        }

        hospital.Name = input.Name.Trim();
        hospital.Location = input.Location.Trim();
        hospital.Phone = input.Phone.Trim();
        await _context.SaveChangesAsync();
        return Ok(hospital);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHospital(int id)
    {
        var hospital = await _context.Hospitals.FirstOrDefaultAsync(item => item.Id == id);
        if (hospital is null)
        {
            return NotFound(new { message = "Hospital not found." });
        }

        _context.Hospitals.Remove(hospital);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "This hospital cannot be removed because related records reference it." });
        }

        return NoContent();
    }

    private static HospitalProfileResponseDto ToProfileResponse(Hospital hospital) => new()
    {
        Id = hospital.Id,
        UserId = hospital.UserId,
        Email = hospital.User?.Email ?? string.Empty,
        HospitalName = hospital.Name,
        Location = hospital.Location,
        ContactInfo = hospital.Phone
    };
}
