using System.Globalization;

namespace BloodBridge.API.Services;

public readonly record struct GeoCoordinate(double Latitude, double Longitude);

public sealed class GeographicService
{
    public bool TryParseCoordinates(string? value, out GeoCoordinate coordinate)
    {
        coordinate = default;
        var parts = value?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts is not { Length: 2 }
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
            || latitude is < -90 or > 90
            || longitude is < -180 or > 180)
        {
            return false;
        }

        coordinate = new GeoCoordinate(latitude, longitude);
        return true;
    }

    public bool IsValidCoordinates(string? value) => TryParseCoordinates(value, out _);

    public double? CalculateDistanceKm(string? first, string? second)
    {
        return TryParseCoordinates(first, out var firstCoordinate)
            && TryParseCoordinates(second, out var secondCoordinate)
            ? CalculateDistanceKm(firstCoordinate, secondCoordinate)
            : null;
    }

    public double CalculateDistanceKm(GeoCoordinate first, GeoCoordinate second)
    {
        const double earthRadiusKm = 6371.0;
        var latitudeDelta = ToRadians(second.Latitude - first.Latitude);
        var longitudeDelta = ToRadians(second.Longitude - first.Longitude);
        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2)
            + Math.Cos(ToRadians(first.Latitude))
            * Math.Cos(ToRadians(second.Latitude))
            * Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return Math.Round(2 * earthRadiusKm * Math.Asin(Math.Sqrt(Math.Min(1, haversine))), 2);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
