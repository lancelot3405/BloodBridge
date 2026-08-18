using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class Donor
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string BloodGroup { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public DateTime? LastDonationDate { get; set; }

    [JsonIgnore]
    public ICollection<DonorMatch> DonorMatches { get; set; } = new List<DonorMatch>();

    [JsonIgnore]
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    [JsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
