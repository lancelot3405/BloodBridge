using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class BloodRequest
{
    public int Id { get; set; }

    [Required]
    public int HospitalId { get; set; }

    [Required, StringLength(3)]
    public string BloodGroup { get; set; } = string.Empty;

    [Range(1, 100)]
    public int UnitsRequired { get; set; }

    [Required, StringLength(20)]
    public string Urgency { get; set; } = "Normal";

    [Required]
    public DateTime RequiredDate { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Hospital? Hospital { get; set; }

    [JsonIgnore]
    public ICollection<DonorMatch> DonorMatches { get; set; } = new List<DonorMatch>();

    [JsonIgnore]
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    [JsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
