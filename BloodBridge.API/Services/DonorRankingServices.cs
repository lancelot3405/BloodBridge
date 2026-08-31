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
