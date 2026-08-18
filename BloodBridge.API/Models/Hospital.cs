using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class Hospital
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    [JsonIgnore]
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
