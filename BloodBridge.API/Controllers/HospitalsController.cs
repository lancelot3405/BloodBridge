using BloodBridge.API.Data;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/hospitals")]
public class HospitalsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;

    public HospitalsController(BloodBridgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hospital>>> GetHospitals()
    {
        return await _context.Hospitals.AsNoTracking().ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Hospital>> CreateHospital(Hospital hospital)
    {
        _context.Hospitals.Add(hospital);
        await _context.SaveChangesAsync();

        return Created($"/api/hospitals/{hospital.Id}", hospital);
    }
}
