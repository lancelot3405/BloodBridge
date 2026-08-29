namespace BloodBridge.Web.ViewModels;

public sealed class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public sealed class HospitalViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public sealed class DonorViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime? LastDonationDate { get; set; }
}

public sealed class BloodRequestViewModel
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public int UnitsRequired { get; set; }
    public string Urgency { get; set; } = string.Empty;
    public DateTime RequiredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? AcceptedDonorId { get; set; }
}

public sealed class MatchViewModel
{
    public int DonorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public double? DistanceKm { get; set; }
    public bool SameLocationAsHospital { get; set; }
}

public sealed class CreateRequestViewModel
{
    public int HospitalId { get; set; }
    public string BloodGroup { get; set; } = "O+";
    public int UnitsRequired { get; set; } = 1;
    public string Urgency { get; set; } = "Normal";
    public DateTime RequiredDate { get; set; } = DateTime.Today.AddDays(1);
}

public sealed class ProfileViewModel
{
    public DonorViewModel? Donor { get; set; }
    public HospitalProfileViewModel? Hospital { get; set; }
}

public sealed class HospitalProfileViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
}

public sealed class UpdateDonorProfileViewModel
{
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public sealed class UpdateHospitalProfileViewModel
{
    public string HospitalName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
}

public sealed class DashboardViewModel
{
    public IReadOnlyList<BloodRequestViewModel> Requests { get; set; } = [];
    public IReadOnlyList<HospitalViewModel> Hospitals { get; set; } = [];
}

public sealed class DonorDashboardViewModel
{
    public DonorViewModel? Profile { get; set; }
    public IReadOnlyList<BloodRequestViewModel> Requests { get; set; } = [];
}

public sealed class HospitalDashboardViewModel
{
    public HospitalProfileViewModel? Profile { get; set; }
    public IReadOnlyList<BloodRequestViewModel> Requests { get; set; } = [];
}

public sealed class AdminStatsViewModel
{
    public int TotalDonors { get; set; }
    public int ActiveBloodRequests { get; set; }
    public int FulfilledRequests { get; set; }
    public int TotalHospitals { get; set; }
    public Dictionary<string, int> RequestsByBloodGroup { get; set; } = new();
}

public sealed class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class AdminUserPageViewModel
{
    public IReadOnlyList<AdminUserViewModel> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class AdminDashboardViewModel
{
    public AdminStatsViewModel Stats { get; set; } = new();
    public AdminUserPageViewModel Users { get; set; } = new();
    public IReadOnlyList<BloodRequestViewModel> Requests { get; set; } = [];
}

public sealed class NotificationViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
