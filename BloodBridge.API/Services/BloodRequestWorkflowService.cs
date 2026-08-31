using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Services;

public sealed class BloodRequestWorkflowService
{
    private readonly BloodBridgeDbContext _context;
    private readonly BloodCompatibilityService _compatibilityService;
    private readonly NotificationService _notificationService;

    public BloodRequestWorkflowService(
        BloodBridgeDbContext context,
        BloodCompatibilityService compatibilityService,
        NotificationService notificationService)
    {
        _context = context;
        _compatibilityService = compatibilityService;
        _notificationService = notificationService;
    }

    public async Task<WorkflowResult> TransitionAsync(
        int requestId,
        UpdateBloodRequestStatusDto input,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var request = await _context.BloodRequests
            .Include(item => item.Hospital)
            .FirstOrDefaultAsync(item => item.Id == requestId, cancellationToken);

        if (request is null)
        {
            return WorkflowResult.NotFound("Blood request not found.");
        }

        if (!BloodRequestStatuses.TryNormalize(request.Status, out var currentStatus))
        {
            return WorkflowResult.Invalid("The stored blood request status is invalid.");
        }

        if (!BloodRequestStatuses.TryNormalize(input.Status, out var targetStatus))
        {
            return WorkflowResult.Invalid("Invalid status. Allowed statuses are PENDING, MATCHED, DONOR ACCEPTED, DONATION COMPLETED, and FULFILLED.");
        }

        var expectedStatus = BloodRequestStatuses.GetNext(currentStatus);
        var donorAcceptanceShortcut = currentStatus == BloodRequestStatuses.Pending
            && targetStatus == BloodRequestStatuses.DonorAccepted;
        if (!donorAcceptanceShortcut
            && (expectedStatus is null || !string.Equals(expectedStatus, targetStatus, StringComparison.Ordinal)))
        {
            return WorkflowResult.Invalid($"Invalid status transition. The next status after {currentStatus} is {expectedStatus ?? "none"}.");
        }

        if (targetStatus == BloodRequestStatuses.DonorAccepted)
        {
            if (input.DonorId is null)
            {
                return WorkflowResult.Invalid("DonorId is required when accepting a donor.");
            }

            var donor = await _context.Donors
                .FirstOrDefaultAsync(item => item.Id == input.DonorId.Value, cancellationToken);

            if (donor is null)
            {
                return WorkflowResult.Invalid("The specified donor does not exist.");
            }

            if (!donor.IsAvailable || !_compatibilityService.IsCompatible(request.BloodGroup, donor.BloodGroup))
            {
                return WorkflowResult.Invalid("The specified donor is unavailable or incompatible with this request.");
            }

            if (donorAcceptanceShortcut)
            {
                // The donor UI can accept directly from PENDING. Record the
                // intermediate MATCHED step before moving to DONOR ACCEPTED.
                request.Status = BloodRequestStatuses.Matched;
                await CreateMatchesAndNotificationsAsync(request, cancellationToken);
            }

            request.AcceptedDonorId = donor.Id;
        }

        if (targetStatus == BloodRequestStatuses.Matched)
        {
            await CreateMatchesAndNotificationsAsync(request, cancellationToken);
        }

        if (targetStatus is BloodRequestStatuses.DonationCompleted or BloodRequestStatuses.Fulfilled)
        {
            if (request.AcceptedDonorId is null)
            {
                return WorkflowResult.Invalid("A donor must be accepted before recording a donation.");
            }

            await EnsureDonationAsync(request, targetStatus, input.DonationDate, cancellationToken);
        }

        request.Status = targetStatus;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return WorkflowResult.Success(request);
    }

    private async Task CreateMatchesAndNotificationsAsync(BloodRequest request, CancellationToken cancellationToken)
    {
        var donors = await _context.Donors
            .AsNoTracking()
            .Where(donor => donor.IsAvailable)
            .ToListAsync(cancellationToken);

        foreach (var donor in _compatibilityService.FindCompatibleDonors(
                     request.BloodGroup, donors, request.Hospital?.Location))
        {
            var matchExists = await _context.DonorMatches.AnyAsync(
                match => match.BloodRequestId == request.Id && match.DonorId == donor.Id,
                cancellationToken);
            if (!matchExists)
            {
                _context.DonorMatches.Add(new DonorMatch
                {
                    DonorId = donor.Id,
                    BloodRequestId = request.Id,
                    MatchScore = _compatibilityService.CalculateDistanceKm(donor.Location, request.Hospital?.Location) is double distance
                        ? (decimal?)Math.Max(0, 100 - Math.Min(distance, 100))
                        : null,
                    Status = "Suggested"
                });
            }

            
            var message = $"New {request.BloodGroup} blood request near you. Can you donate?";
            await _notificationService.NotifyDonorAsync(donor, "New donor match", message, cancellationToken);
        }
    }

    public async Task<WorkflowResult> RecordDonationAsync(
        CreateDonationDto input,
        CancellationToken cancellationToken)
    {
        var request = await _context.BloodRequests
            .FirstOrDefaultAsync(item => item.Id == input.BloodRequestId, cancellationToken);

        if (request is null)
        {
            return WorkflowResult.NotFound("Blood request not found.");
        }

        if (!string.Equals(request.Status, BloodRequestStatuses.DonorAccepted, StringComparison.OrdinalIgnoreCase))
        {
            return WorkflowResult.Invalid("A donation can only be recorded after a donor has been accepted.");
        }

        if (request.AcceptedDonorId != input.DonorId)
        {
            return WorkflowResult.Invalid("The donor is not the donor accepted for this request.");
        }

        var donationAlreadyExists = await _context.Donations
            .AnyAsync(item => item.BloodRequestId == input.BloodRequestId && item.DonorId == input.DonorId, cancellationToken);

        if (donationAlreadyExists)
        {
            return WorkflowResult.Conflict("A donation has already been recorded for this request and donor.");
        }

        var donor = await _context.Donors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == input.DonorId, cancellationToken);

        if (donor is null || !_compatibilityService.IsCompatible(request.BloodGroup, donor.BloodGroup))
        {
            return WorkflowResult.Invalid("The specified donor does not exist or is incompatible with this request.");
        }

        var donation = new Donation
        {
            DonorId = donor.Id,
            BloodRequestId = request.Id,
            HospitalId = request.HospitalId,
            BloodGroup = request.BloodGroup,
            DonationDate = input.DonationDate?.ToUniversalTime() ?? DateTime.UtcNow,
            Status = BloodRequestStatuses.DonationCompleted
        };

        _context.Donations.Add(donation);
        request.Status = BloodRequestStatuses.DonationCompleted;
        await _context.SaveChangesAsync(cancellationToken);

        return WorkflowResult.Success(request, donation);
    }

    private async Task EnsureDonationAsync(
        BloodRequest request,
        string status,
        DateTime? donationDate,
        CancellationToken cancellationToken)
    {
        var donation = await _context.Donations
            .FirstOrDefaultAsync(item => item.BloodRequestId == request.Id && item.DonorId == request.AcceptedDonorId, cancellationToken);

        if (donation is null)
        {
            _context.Donations.Add(new Donation
            {
                DonorId = request.AcceptedDonorId!.Value,
                BloodRequestId = request.Id,
                HospitalId = request.HospitalId,
                BloodGroup = request.BloodGroup,
                DonationDate = donationDate?.ToUniversalTime() ?? DateTime.UtcNow,
                Status = status
            });

            return;
        }

        donation.Status = status;
        if (donationDate.HasValue && status == BloodRequestStatuses.DonationCompleted)
        {
            donation.DonationDate = donationDate.Value.ToUniversalTime();
        }
    }
}

public sealed class WorkflowResult
{
    private WorkflowResult(bool succeeded, int statusCode, string? error, BloodRequest? request, Donation? donation)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        Error = error;
        Request = request;
        Donation = donation;
    }

    public bool Succeeded { get; }
    public int StatusCode { get; }
    public string? Error { get; }
    public BloodRequest? Request { get; }
    public Donation? Donation { get; }

    public static WorkflowResult Success(BloodRequest request, Donation? donation = null) =>
        new(true, StatusCodes.Status200OK, null, request, donation);

    public static WorkflowResult NotFound(string error) =>
        new(false, StatusCodes.Status404NotFound, error, null, null);

    public static WorkflowResult Invalid(string error) =>
        new(false, StatusCodes.Status400BadRequest, error, null, null);

    public static WorkflowResult Conflict(string error) =>
        new(false, StatusCodes.Status409Conflict, error, null, null);
}
