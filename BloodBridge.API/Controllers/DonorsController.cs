using BloodBridge.API.Data;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/donors")]
public class DonorsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;

    public DonorsController(BloodBridgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Donor>>> GetDonors()
    {
        return await _context.Donors.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id}")]
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
    public async Task<ActionResult<Donor>> CreateDonor(Donor donor)
    {
        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDonor), new { id = donor.Id }, donor);
    }
}
