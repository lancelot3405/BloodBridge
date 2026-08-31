using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Models;

public class Donor
{
    public int Id { get; set; }

    // Links a donor profile to the authenticated Identity account.
    [JsonIgnore]
    [Required]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string BloodGroup { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Coordinates]
    public string Location { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public DateTime? LastDonationDate { get; set; }

    [JsonIgnore]
    [NotMapped]
    public double? DistanceKmForRanking { get; set; }

    [JsonIgnore]
    public ICollection<DonorMatch> DonorMatches { get; set; } = new List<DonorMatch>();

    [JsonIgnore]
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    [JsonIgnore]
    public GamificationProfile? GamificationProfile { get; set; }

}
