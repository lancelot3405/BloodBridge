using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class Requester
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string ContactNumber { get; set; } = string.Empty;
}
