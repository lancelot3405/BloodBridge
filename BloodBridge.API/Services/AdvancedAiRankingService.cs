using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BloodBridge.API.Data;
using BloodBridge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Services;

public sealed class AdvancedAiRankingService : IDonorRankingService
{
    private const string DefaultRankingEndpoint = "http://localhost:8000/rank-donors";
    private readonly BloodBridgeDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AdvancedAiRankingService(
        BloodBridgeDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<Donor> RankDonors(List<Donor> eligibleDonors) =>
        RankDonorsAsync(eligibleDonors).GetAwaiter().GetResult();

    // Keep the existing synchronous interface working while the ML call uses async HTTP.
    public async Task<List<Donor>> RankDonorsAsync(
        List<Donor> eligibleDonors,
        CancellationToken cancellationToken = default)
    {
        var rankedDtos = await RankDonorDtosAsync(eligibleDonors, cancellationToken);
        var donorsById = eligibleDonors.ToDictionary(donor => donor.Id);

        return rankedDtos
            .Where(rankedDonor => donorsById.ContainsKey(rankedDonor.Id))
            .Select(rankedDonor => donorsById[rankedDonor.Id])
            .Concat(eligibleDonors.Where(donor => rankedDtos.All(ranked => ranked.Id != donor.Id)))
            .ToList();
    }

    // Send donor features to Python and return its sorted result with explanations.
    public async Task<List<RankedDonorDto>> RankDonorDtosAsync(
        List<Donor> eligibleDonors,
        CancellationToken cancellationToken = default)
    {
        if (eligibleDonors.Count == 0)
        {
            return [];
        }

        var endpoint = _configuration["Ranking:AiEndpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = DefaultRankingEndpoint;
        }

        var donorIds = eligibleDonors.Select(donor => donor.Id).ToArray();
        var gamificationScores = await _context.GamificationProfiles
            .AsNoTracking()
            .Where(profile => donorIds.Contains(profile.DonorId))
            .ToDictionaryAsync(profile => profile.DonorId, profile => profile.ImpactScore, cancellationToken);
        var donorMatchHistory = await _context.DonorMatches
            .AsNoTracking()
            .Where(match => donorIds.Contains(match.DonorId))
            .Select(match => new { match.DonorId, match.Status })
            .ToListAsync(cancellationToken);

        // Use existing V1 match history and V2 impact score as the ML behavior features.
        var request = eligibleDonors.Select(donor =>
        {
            var donorMatches = donorMatchHistory
                .Where(match => match.DonorId == donor.Id)
                .ToList();

            return new DonorRankingRequestDto
            {
                Id = donor.Id,
                Distance = donor.DistanceKmForRanking ?? 999,
                Received = donorMatches.Count,
                Accepted = donorMatches.Count(match => string.Equals(
                    match.Status,
                    "Accepted",
                    StringComparison.OrdinalIgnoreCase)),
                Xp = gamificationScores.GetValueOrDefault(donor.Id, 0)
            };
        }).ToList();

        try
        {
            var client = _httpClientFactory.CreateClient("DonorRankingAi");
            var response = await client.PostAsJsonAsync(endpoint, request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var rankedDonors = await response.Content.ReadFromJsonAsync<List<RankedDonorDto>>(
                cancellationToken: cancellationToken);

            return rankedDonors ?? BuildFallbackDtos(eligibleDonors);
        }
        catch (HttpRequestException)
        {
            return BuildFallbackDtos(eligibleDonors);
        }
        catch (JsonException)
        {
            return BuildFallbackDtos(eligibleDonors);
        }
        catch (TaskCanceledException)
        {
            return BuildFallbackDtos(eligibleDonors);
        }
    }

    // Use V1 distance ordering when the optional Python service is not running.
    private static List<RankedDonorDto> BuildFallbackDtos(List<Donor> eligibleDonors)
    {
        return new StandardRankingService()
            .RankDonors(eligibleDonors)
            .Select(donor => new RankedDonorDto
            {
                Id = donor.Id,
                Distance = donor.DistanceKmForRanking ?? 999,
                Probability = 0,
                Explanation = "Python ranking service unavailable; standard distance ranking used."
            })
            .ToList();
    }
}

// This DTO is the small request contract shared by the C# API and Python service.
public sealed class DonorRankingRequestDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("distance")]
    public double Distance { get; init; }

    [JsonPropertyName("received")]
    public int Received { get; init; }

    [JsonPropertyName("accepted")]
    public int Accepted { get; init; }

    [JsonPropertyName("xp")]
    public int Xp { get; init; }
}

// This DTO contains the Python prediction and its explanation for future V2 screens.
public sealed class RankedDonorDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("distance")]
    public double Distance { get; init; }

    [JsonPropertyName("received")]
    public int Received { get; init; }

    [JsonPropertyName("accepted")]
    public int Accepted { get; init; }

    [JsonPropertyName("xp")]
    public double Xp { get; init; }

    [JsonPropertyName("response_rate")]
    public double ResponseRate { get; init; }

    [JsonPropertyName("probability")]
    public double Probability { get; init; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; init; } = string.Empty;
}
