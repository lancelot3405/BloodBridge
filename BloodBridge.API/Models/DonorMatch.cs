using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class DonorMatch
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required]
    public int BloodRequestId { get; set; }

    public decimal? MatchScore { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Suggested";

    [JsonIgnore]
    public Donor? Donor { get; set; }

    [JsonIgnore]
    public BloodRequest? BloodRequest { get; set; }
}
