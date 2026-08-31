using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/donors")]
[Authorize]
public class DonorsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DonorsController(BloodBridgeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Donor>>> GetDonors()
    {
        return await _context.Donors.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Donor>> GetDonor(int id)
    {
        var donor = await _context.Donors.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);

        if (donor == null)
        {
            return NotFound(new { message = "Donor not found." });
        }

        return donor;
    }

    [HttpPost]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<Donor>> CreateDonor(Donor donor)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        if (await _context.Donors.AnyAsync(item => item.UserId == _currentUser.UserId))
        {
            return Conflict(new { message = "The authenticated donor already has a profile." });
        }

        donor.UserId = _currentUser.UserId;

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDonor), new { id = donor.Id }, donor);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<DonorProfileResponseDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var donor = await _context.Donors
            .AsNoTracking()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        return donor is null ? NotFound(new { message = "Donor profile not found." }) : Ok(ToProfileResponse(donor));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<DonorProfileResponseDto>> UpdateMyProfile(
        UpdateDonorProfileDto input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized();
        }

        var donor = await _context.Donors
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == _currentUser.UserId, cancellationToken);
        if (donor is null)
        {
            return NotFound(new { message = "Donor profile not found." });
        }

        donor.Phone = input.Phone.Trim();
        donor.Location = input.Location.Trim();
        donor.IsAvailable = input.IsAvailable;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToProfileResponse(donor));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Donor,Admin")]
    public async Task<ActionResult<Donor>> UpdateDonor(int id, UpdateDonorDto input)
    {
        var donor = await _context.Donors.FirstOrDefaultAsync(item => item.Id == id);
        if (donor is null)
        {
            return NotFound(new { message = "Donor not found." });
        }

        var userId = _currentUser.UserId;
        if (!User.IsInRole(ApplicationRoles.Admin) && !string.Equals(donor.UserId, userId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        donor.Name = input.Name.Trim();
        donor.Phone = input.Phone.Trim();
        donor.Location = input.Location.Trim();
        donor.IsAvailable = input.IsAvailable;
        await _context.SaveChangesAsync();

        return Ok(donor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Donor,Admin")]
    public async Task<IActionResult> DeleteDonor(int id)
    {
        var donor = await _context.Donors.FirstOrDefaultAsync(item => item.Id == id);
        if (donor is null)
        {
            return NotFound(new { message = "Donor not found." });
        }

        var userId = _currentUser.UserId;
        if (!User.IsInRole(ApplicationRoles.Admin) && !string.Equals(donor.UserId, userId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        _context.Donors.Remove(donor);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "This donor cannot be removed because donation or matching records reference the profile." });
        }

        return NoContent();
    }

    private static DonorProfileResponseDto ToProfileResponse(Donor donor) => new()
    {
        Id = donor.Id,
        UserId = donor.UserId,
        Email = donor.User?.Email ?? string.Empty,
        Name = donor.Name,
        BloodGroup = donor.BloodGroup,
        Phone = donor.Phone,
        Location = donor.Location,
        IsAvailable = donor.IsAvailable,
        LastDonationDate = donor.LastDonationDate
    };
}
