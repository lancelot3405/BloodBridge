using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class Donation
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required]
    public int BloodRequestId { get; set; }

    [Required]
    public int HospitalId { get; set; }

    [Required]
    public DateTime DonationDate { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    [JsonIgnore]
    public Donor? Donor { get; set; }

    [JsonIgnore]
    public BloodRequest? BloodRequest { get; set; }

    [JsonIgnore]
    public Hospital? Hospital { get; set; }
}
