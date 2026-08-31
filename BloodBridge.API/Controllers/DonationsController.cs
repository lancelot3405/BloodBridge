using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/donations")]
[Authorize]
public sealed class DonationsController : ControllerBase
{
    private readonly BloodRequestWorkflowService _workflowService;

    public DonationsController(BloodRequestWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpPost]
    public async Task<ActionResult<DonationResponseDto>> Create(
        CreateDonationDto input,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.RecordDonationAsync(input, cancellationToken);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        var donation = result.Donation!;
        return Created($"/api/donations/{donation.Id}", ToResponse(donation));
    }

    private static DonationResponseDto ToResponse(Donation donation) => new()
    {
        Id = donation.Id,
        DonorId = donation.DonorId,
        BloodRequestId = donation.BloodRequestId,
        HospitalId = donation.HospitalId,
        BloodGroup = donation.BloodGroup,
        DonationDate = donation.DonationDate,
        Status = donation.Status
    };
}
