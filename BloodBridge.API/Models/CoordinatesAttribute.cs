using System.ComponentModel.DataAnnotations;
using BloodBridge.API.Services;

namespace BloodBridge.API.Models;

public sealed class CoordinatesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return new GeographicService().IsValidCoordinates(value as string)
            ? ValidationResult.Success
            : new ValidationResult("Location must contain latitude and longitude as `latitude,longitude`.");
    }
}
