using BloodBridge.API.Models;
using BloodBridge.API.Services;

namespace BloodBridge.Tests;

public sealed class GeographicServiceTests
{
    private readonly GeographicService _service = new();

    [Fact]
    public void SameCoordinatesHaveZeroDistance()
    {
        Assert.Equal(0, _service.CalculateDistanceKm("23.2599,77.4126", "23.2599,77.4126"));
    }

    [Fact]
    public void OneDegreeOfLatitudeIsApproximately111Kilometers()
    {
        var distance = _service.CalculateDistanceKm("0,0", "1,0");

        Assert.NotNull(distance);
        Assert.InRange(distance.Value, 111.1, 111.3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bhopal")]
    [InlineData("91,77")]
    [InlineData("23,181")]
    [InlineData("23.2")]
    public void InvalidCoordinateStringsAreRejected(string? location)
    {
        Assert.False(_service.IsValidCoordinates(location));
        Assert.Null(_service.CalculateDistanceKm(location, "23.2599,77.4126"));
    }

    [Fact]
    public void StandardRankingSortsClosestDonorsFirst()
    {
        var donors = new List<Donor>
        {
            new() { Id = 1, Name = "Far", DistanceKmForRanking = 10 },
            new() { Id = 2, Name = "Closest", DistanceKmForRanking = 1 },
            new() { Id = 3, Name = "Unknown", DistanceKmForRanking = null }
        };

        var ranked = new StandardRankingService().RankDonors(donors);

        Assert.Equal(new[] { 2, 1, 3 }, ranked.Select(donor => donor.Id));
    }
}
