using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace BloodBridge.API.Models;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public Donor? Donor { get; set; }

    [JsonIgnore]
    public Hospital? Hospital { get; set; }

    [JsonIgnore]
    public Requester? Requester { get; set; }
}
