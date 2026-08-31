using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public sealed class ImpactActivityLog
{
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required, StringLength(150)]
    public string ActivityName { get; set; } = string.Empty;

    public int PointsEarned { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Donor Donor { get; set; } = null!;
}
