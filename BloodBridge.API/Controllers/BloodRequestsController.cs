using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/bloodrequests")]
[Authorize]
public class BloodRequestsController : ControllerBase
{
    private readonly BloodBridgeDbContext _context;
    private readonly BloodRequestWorkflowService _workflowService;
    private readonly BloodCompatibilityService _compatibilityService;
    private readonly ICurrentUserService _currentUser;

    public BloodRequestsController(
        BloodBridgeDbContext context,
        BloodRequestWorkflowService workflowService,
        BloodCompatibilityService compatibilityService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _workflowService = workflowService;
        _compatibilityService = compatibilityService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BloodRequest>>> GetBloodRequests()
    {
        var query = _context.BloodRequests.AsNoTracking();
        if (User.IsInRole(ApplicationRoles.Requester))
        {
            query = query.Where(request => request.RequesterId == _currentUser.UserId);
        }

        return await query.ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Requester,Hospital,Admin")]
    public async Task<ActionResult<BloodRequest>> CreateBloodRequest(CreateBloodRequestDto input)
    {
        var hospitalExists = await _context.Hospitals.AnyAsync(hospital => hospital.Id == input.HospitalId);

        if (!hospitalExists)
        {
            return BadRequest(new { message = "The specified HospitalId does not exist." });
        }

        if (!_compatibilityService.IsKnownBloodGroup(input.BloodGroup))
        {
            return BadRequest(new { message = "The specified BloodGroup is invalid." });
        }

        var bloodRequest = new BloodRequest
        {
            HospitalId = input.HospitalId,
            RequesterId = User.IsInRole(ApplicationRoles.Requester) ? _currentUser.UserId : null,
            BloodGroup = input.BloodGroup.Trim().ToUpperInvariant(),
            UnitsRequired = input.UnitsRequired,
            Urgency = input.Urgency.Trim().ToUpperInvariant(),
            RequiredDate = input.RequiredDate!.Value,
            Status = BloodRequestStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.BloodRequests.Add(bloodRequest);
        await _context.SaveChangesAsync();

        return Created($"/api/bloodrequests/{bloodRequest.Id}", bloodRequest);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<BloodRequestStatusResponseDto>> UpdateStatus(
        int id,
        UpdateBloodRequestStatusDto input,
        CancellationToken cancellationToken)
    {
        if (string.Equals(input.Status?.Trim(), BloodRequestStatuses.Fulfilled, StringComparison.OrdinalIgnoreCase)
            && !User.IsInRole(ApplicationRoles.Hospital)
            && !User.IsInRole(ApplicationRoles.Admin))
        {
            return Forbid();
        }

        var result = await _workflowService.TransitionAsync(id, input, cancellationToken);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return Ok(ToResponse(result.Request!));
    }

    private static BloodRequestStatusResponseDto ToResponse(BloodRequest request) => new()
    {
        Id = request.Id,
        HospitalId = request.HospitalId,
        BloodGroup = request.BloodGroup,
        UnitsRequired = request.UnitsRequired,
        Urgency = request.Urgency,
        RequiredDate = request.RequiredDate,
        Status = request.Status,
        CreatedAt = request.CreatedAt,
        AcceptedDonorId = request.AcceptedDonorId
    };
}
