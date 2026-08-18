using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required]
    public int BloodRequestId { get; set; }

    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Donor? Donor { get; set; }

    [JsonIgnore]
    public BloodRequest? BloodRequest { get; set; }
}
