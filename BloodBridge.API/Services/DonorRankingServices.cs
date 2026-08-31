using System.Net.Http.Json;
using System.Text.Json;
using BloodBridge.API.Models;

namespace BloodBridge.API.Services;

public interface IDonorRankingService
{
    List<Donor> RankDonors(List<Donor> eligibleDonors);
}

public sealed class StandardRankingService : IDonorRankingService
{
    public List<Donor> RankDonors(List<Donor> eligibleDonors) =>
        eligibleDonors
            .OrderBy(donor => donor.DistanceKmForRanking ?? double.MaxValue)
            .ThenBy(donor => donor.Name)
            .ToList();
}

public sealed class AdvancedAiRankingService : IDonorRankingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AdvancedAiRankingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<Donor> RankDonors(List<Donor> eligibleDonors) =>
        RankDonorsAsync(eligibleDonors).GetAwaiter().GetResult();

    public async Task<List<Donor>> RankDonorsAsync(
        List<Donor> eligibleDonors,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["Ranking:AiEndpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return new StandardRankingService().RankDonors(eligibleDonors);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DonorRankingAi");
            var response = await client.PostAsJsonAsync(endpoint, eligibleDonors, cancellationToken);
            response.EnsureSuccessStatusCode();
            var rankedIds = await response.Content.ReadFromJsonAsync<List<int>>(cancellationToken: cancellationToken);
            if (rankedIds is null)
            {
                return new StandardRankingService().RankDonors(eligibleDonors);
            }

            var byId = eligibleDonors.ToDictionary(donor => donor.Id);
            return rankedIds
                .Where(byId.ContainsKey)
                .Select(id => byId[id])
                .Concat(eligibleDonors.Where(donor => !rankedIds.Contains(donor.Id)))
                .ToList();
        }
        catch (HttpRequestException)
        {
            return new StandardRankingService().RankDonors(eligibleDonors);
        }
        catch (JsonException)
        {
            return new StandardRankingService().RankDonors(eligibleDonors);
        }
        catch (TaskCanceledException)
        {
            return new StandardRankingService().RankDonors(eligibleDonors);
        }
    }
}
